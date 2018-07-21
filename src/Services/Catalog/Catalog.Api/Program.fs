module Identity.Api.App

open System
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.Identity.EntityFrameworkCore
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Configuration
open Saturn
open MongoDB.Driver
open Catalog.Api.Controllers

let envGetOrElse key elseVal =
    match System.Environment.GetEnvironmentVariable(key) with
    | null -> elseVal
    | res -> res

// ---------------------------------
// Configuration
// ---------------------------------
let configuration =
    ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build()



let configureServices (services : IServiceCollection) =
    services.AddSingleton<MongoClient>(fun opt -> MongoClient(opt.GetService<>)) |> ignore
    services



let topRouter = router {
    forward "/products" Products.controller
}

let app = application {
    use_router topRouter
    url (envGetOrElse "API_URL" "http://0.0.0.0:8085/")
    service_config (configureServices)
}

[<EntryPoint>]
let main _ =
    run app
    0
