// Learn more about F# at http://fsharp.org

open System
open Argu
open Bogus
open Catalog.Api.Model
open MongoDB.Driver
open MongoDB.Driver.Core.Configuration
open Catalog.Api.Services
open System.Numerics

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
      let collection = db.GetCollection<Product>("products")
      let p = generate(results.GetResult(ProductsQuantity, defaultValue = 100))
      collection.InsertManyAsync(p) |> Async.AwaitTask |> Async.RunSynchronously
    else
      parser.PrintUsage() |> printfn "%s"
    0 // return an integer exit code
