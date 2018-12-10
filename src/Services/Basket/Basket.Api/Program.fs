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
open Microsoft.IdentityModel.Tokens
open System.Text
open StackExchange.Redis
open Basket.Domain.Storage


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
    let pathbase = Environment.getOrElse "PATH_BASE" ""
    if String.IsNullOrEmpty(pathbase) |> not then
      app.UsePathBase(PathString(pathbase)) |> ignore

let configureServices (services : IServiceCollection) =
    services
        .AddResponseCaching()
        .AddGiraffe() |> ignore
    services.AddAuthentication(fun opt ->
                                opt.DefaultAuthenticateScheme <- JwtBearerDefaults.AuthenticationScheme
                                opt.DefaultChallengeScheme <- JwtBearerDefaults.AuthenticationScheme
                              )
            .AddJwtBearer(fun cfg ->
                            let issuer = configuration.["Service:Jwt:Issuer"]
                            let secret = SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.["Service:Jwt:SecretKey"]))
                            cfg.IncludeErrorDetails <- true
                            cfg.TokenValidationParameters <- TokenValidationParameters(ValidateAudience = true, ValidateIssuer = true, ValidIssuer = issuer, ValidAudience = issuer, IssuerSigningKey = secret)
                          ) |> ignore
    services.AddSingleton<IConnectionMultiplexer>(fun opt -> ConnectionMultiplexer.Connect(configuration.["Service:Database:Connection"]) :> IConnectionMultiplexer) |> ignore
    services.AddTransient<ICustomerBasketRepository>(fun opt -> CustomerBasket.storage (opt.GetService<IConnectionMultiplexer>())) |> ignore

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
