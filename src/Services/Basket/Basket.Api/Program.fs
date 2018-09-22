module Basket.Api.App

open Microsoft.Extensions.DependencyInjection
open Saturn
open MongoDB.Driver
open Microsoft.AspNetCore.Cors.Infrastructure
open StackExchange.Redis
open Basket.Api.Controller
open Basket.Domain.Storage
open Basket.Domain.Storage
open StackExchange.Redis

let configureServices (services : IServiceCollection) =
    services.AddSingleton<IConnectionMultiplexer>(fun opt -> ConnectionMultiplexer.Connect(Environment.getOrElse "REDIS_CONNECTION" "localhost") :> IConnectionMultiplexer) |> ignore
    services.AddTransient<ICustomerBasketRepository>(fun opt -> CustomerBasket.storage (opt.GetService<IConnectionMultiplexer>())) |> ignore
    services

let topRouter = router {
    pipe_through (Auth.requireAuthentication JWT)
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
    use_jwt_authentication (Environment.getOrElse "JWT_SECRET" "ksX9NWD820UKt2T9UkC3jJAaS7W0vvyj") (Environment.getOrElse "JWT_ISSUER" "http://auth.api")
    use_cors ("default")(corsPolicy)
    url (Environment.getOrElse "API_URL" "http://0.0.0.0:8086/")
    service_config (configureServices)
}

[<EntryPoint>]
let main _ =
    run app
    0
