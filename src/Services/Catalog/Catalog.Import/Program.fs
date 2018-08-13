// Learn more about F# at http://fsharp.org

open System
open Argu
open Bogus
open Catalog.Api.Model
open MongoDB.Driver
open MongoDB.Driver.Core.Configuration
open Catalog.Api.Services
open System.Numerics
open MongoDB.Driver
open MongoDB.Bson
open MongoDB.Driver
open Catalog.Api.Repositories
open MongoDB.Driver
open MongoDB.Driver
open Bogus.Bson
open MongoDB.Driver
open MongoDB.Driver

let inline (=>) k v = k, box v

type Arguments =
    | [<AltCommandLine("-c")>] ConnectionString of connstr:string
    | [<AltCommandLine("-q")>] ProductsQuantity of q:int
with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | ProductsQuantity _ -> "Specify a products quantity to generation"
            | ConnectionString _ -> "Specify a mongodb connectionstring."


let generate (q: int) =
  let f = Faker<Product>()
            .CustomInstantiator(fun f ->
                          { Id = Guid.NewGuid()
                            Slug = f.Lorem.Slug()
                            Name = f.Commerce.ProductName()
                            Brand = f.Company.CompanyName()
                            Description = f.Commerce.ProductAdjective()
                            Details = { Weight = f.Random.Double(); WeightUnits = "kg"; Manufacturer = f.Company.CompanyName(); Color = f.Internet.Color() }
                            Price = f.Random.Double(1., 1000.)
                            AvailableStock = f.Random.Number()
                            PictureUri = f.Image.Food()
                            Tags = [| f.Lorem.Word(); f.Lorem.Word(); f.Lorem.Word();|]}
                        )
  f.Generate(q)

[<EntryPoint>]
let main argv =
    let parser = ArgumentParser.Create<Arguments>(programName = "Catalog.Import")
    let results = parser.Parse(argv, raiseOnUsage = false, ignoreUnrecognized = true)
    if not results.IsUsageRequested then
      let client = MongoClient(results.GetResult(ConnectionString,  defaultValue = "mongodb://127.0.0.1:27017"))
      let db = client.GetDatabase("Catalog")
      db.DropCollectionAsync("products") |> Async.AwaitTask |> Async.RunSynchronously
      let collection = db.GetCollection<Product>("products")
      let p = generate(results.GetResult(ProductsQuantity, defaultValue = 100))
      collection.InsertManyAsync(p) |> Async.AwaitTask |> Async.RunSynchronously
      collection.Indexes.CreateOneAsync(CreateIndexModel<Product>(IndexKeysDefinition<Product>.op_Implicit(BsonDocument(dict [ "Slug" => 1 ])))) |> Async.AwaitTask |> Async.RunSynchronously |> ignore
      collection.Indexes.CreateOneAsync(CreateIndexModel<Product>(IndexKeysDefinition<Product>.op_Implicit(BsonDocument(dict [ "Name" => "text" ])))) |> Async.AwaitTask |> Async.RunSynchronously |> ignore
    else
      parser.PrintUsage() |> printfn "%s"
    0 // return an integer exit code
