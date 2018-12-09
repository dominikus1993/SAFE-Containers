module Basket.Api.App

open System
open System.Threading
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open System.Collections.Generic
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Options
open Microsoft.AspNetCore.Authentication.JwtBearer


let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message


let parsingErrorHandler err = RequestErrors.BAD_REQUEST err

let webApp =
    choose [
        GET >=>
            choose [
                route  "/" >=> handler
            ]
        RequestErrors.notFound (text "Not Found") ]

// ---------------------------------
// Main
// ---------------------------------

let configuration =
  ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build()

let configureApp (app : IApplicationBuilder) =
    app.UseGiraffeErrorHandler(errorHandler)
       .UseStaticFiles()
       .UseResponseCaching()
       .UseGiraffe webApp

let configureServices (services : IServiceCollection) =
    services
        .AddResponseCaching()
        .AddGiraffe() |> ignore

let configureLogging (loggerBuilder : ILoggingBuilder) =
    loggerBuilder.AddFilter(fun lvl -> lvl.Equals LogLevel.Error)
                 .AddConsole()
                 .AddDebug() |> ignore

[<EntryPoint>]
let main _ =
    WebHost.CreateDefaultBuilder()
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0
