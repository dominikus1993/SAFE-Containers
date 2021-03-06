﻿module Catalog.Api.App
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open System
open Microsoft.Extensions.Logging
open Giraffe
open FSharp.Control.Tasks.V2
open MongoDB.Driver
open Catalog.Api.Repositories

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message


let parsingErrorHandler err = RequestErrors.BAD_REQUEST err

let webApp =
    subRoute "/api"
      (choose [
        subRoute "/products" Catalog.Api.Controllers.Products.controller
        subRoute "/tags" Catalog.Api.Controllers.Tags.controller
      ])

// ---------------------------------
// Main
// ---------------------------------


type Startup(configuration: IConfiguration) =
  member __.ConfigureServices (services : IServiceCollection) =
    services
        .AddResponseCaching()
        .AddGiraffe() |> ignore
    services.AddSingleton<IMongoClient>(fun opt -> MongoClient(configuration.["Service:Database:Connection"]) :> IMongoClient) |> ignore
    services.AddTransient<IProductRepository>(fun provider ->
                                                    Product.storage(MongoDb(provider.GetService<IMongoClient>().GetDatabase("Catalog")))
                                            ) |> ignore
    services.AddTransient<ITagsRepository>(fun provider ->
                                                    Tags.storage(MongoDb(provider.GetService<IMongoClient>().GetDatabase("Catalog")))
                                          ) |> ignore
    services.AddAppMetrics( match Environment.getOrElse "PATH_BASE" "" with "" -> None | path -> Some(path))
    services.AddCors() |> ignore

  member __.Configure (app : IApplicationBuilder)
                        (env : IHostingEnvironment)
                        (loggerFactory : ILoggerFactory) =

    app.UseCors(fun policy ->
                  policy.AllowAnyHeader() |> ignore
                  policy.AllowAnyOrigin() |> ignore
                  policy.AllowAnyMethod() |> ignore
                  policy.AllowCredentials() |> ignore
                ) |> ignore
    let pathbase = Environment.getOrElse "PATH_BASE" ""
    if String.IsNullOrEmpty(pathbase) |> not then
      app.UsePathBase(PathString(pathbase)) |> ignore

    app.UseGiraffeErrorHandler(errorHandler)
       .UseStaticFiles()
       .UseResponseCaching()
       .UseGiraffe webApp

let configureLogging (loggerBuilder : ILoggingBuilder) =
    loggerBuilder.AddFilter(fun lvl -> lvl.Equals LogLevel.Error)
                 .AddConsole()
                 .AddDebug() |> ignore

[<EntryPoint>]
let main _ =
    WebHost.CreateDefaultBuilder()
        .UseAppMetrics()
        .UseStartup<Startup>()
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0

