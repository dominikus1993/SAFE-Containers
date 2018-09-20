module Basket.Api.App

open Microsoft.Extensions.DependencyInjection
open Saturn
open MongoDB.Driver
open Microsoft.AspNetCore.Cors.Infrastructure
open StackExchange.Redis
open Basket.Api.Controller

let configureServices (services : IServiceCollection) =
    services.AddSingleton<IConnectionMultiplexer>(fun opt -> ConnectionMultiplexer.Connect(Environment.getOrElse "REDIS_CONNECTION" "localhost") :> IConnectionMultiplexer) |> ignore
    services



let topRouter = router {
    forward "/basket" CustomerBasket.controller
}

let corsPolicy (config: CorsPolicyBuilder) =
  config.AllowAnyHeader() |> ignore
  config.AllowAnyMethod() |> ignore
  config.AllowAnyOrigin() |> ignore
  config.AllowCredentials() |> ignore

let app = application {
    use_router topRouter
    use_pathbase (Environment.getOrElse "PATH_BASE" "")
    use_cors ("default")(corsPolicy)
    url (Environment.getOrElse "API_URL" "http://0.0.0.0:8085/")
    service_config (configureServices)
}

[<EntryPoint>]
let main _ =
    run app
    0
