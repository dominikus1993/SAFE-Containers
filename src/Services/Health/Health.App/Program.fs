module Health.App

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
open Giraffe.GiraffeViewEngine
open Service
open System.Collections.Generic
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Options

let checkView (key)(value) =
  li [_id key] [
    p [] [ encodedText key ]
    p [] [ encodedText value ]
  ]

let serviceCheckView (check: CheckResult) =
  li [_id check.ServiceName] [
    h1 [] [ encodedText check.ServiceName ]
    p [] [ encodedText check.Status.Status ]
    ul [] [
      yield! (check.Status.GeHealthyStatus() |> Seq.map(fun kv -> checkView(kv.Key)(kv.Value)))
      yield! (check.Status.GetUnhealthyStatus() |> Seq.map(fun kv -> checkView(kv.Key)(kv.Value)))
    ]
  ]

let view (model: CheckResult list) =
  div [] [
    h1 [] [ encodedText "HealtChecks" ]
    ul [] [
      yield! model |> List.map(serviceCheckView)
    ]
  ]


let handler =
  fun (next : HttpFunc) (ctx : HttpContext) ->
    task {
      let config = ctx.GetService<IOptions<ServicesConfig>>()
      let! result = config.Value.Services |> Seq.toList |> download
      return! htmlView (view (result |> Seq.toList)) next ctx
    }
// ---------------------------------
// Error handler
// ---------------------------------

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
    services.Configure<ServicesConfig>(configuration) |> ignore
    services.AddDataProtection() |> ignore

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
        .UseUrls("http://localhost:5001")
        .Build()
        .Run()
    0
