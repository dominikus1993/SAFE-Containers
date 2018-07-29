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
open Catalog.Api.Repositories

let configureServices (services : IServiceCollection) =
    services.AddSingleton<IMongoClient>(fun opt -> MongoClient(Environment.getOrElse "MONGO_CONNECTION" "mongodb://127.0.0.1:27017") :> IMongoClient) |> ignore
    services.AddTransient<IProductRepository>(fun provider ->
                                                    Product.storage(MongoDb(provider.GetService<IMongoClient>().GetDatabase("Catalog")))
                                            ) |> ignore
    services



let topRouter = router {
    forward "/products" Products.controller
}

let app = application {
    use_router topRouter
    use_pathbase (Environment.getOrElse "PATH_BASE" "")
    url (Environment.getOrElse "API_URL" "http://0.0.0.0:8085/")
    service_config (configureServices)
}

[<EntryPoint>]
let main _ =
    run app
    0
