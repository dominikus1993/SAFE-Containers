﻿// Learn more about F# at http://fsharp.org

open System
open Argu
open Bogus
open Catalog.Api.Model
open MongoDB.Driver
open MongoDB.Bson
open Akka
open Akka.FSharp
open Akka.Streams
open MongoDB.Driver
open Akka.Configuration
open System.Threading
open Akka.Streams.Dsl
open Akka.Streams.Implementation.Fusing
open Akka.Actor

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

type ProductCollectionActorMsg =
  | GenerateAndStore of q: int
  | Store of Product seq
  | Status of msg: string
  | DropCollection
  | CreateIndexes
  | Check of initialProductsQ: int

let ProductCollectionActor (client: MongoClient) (mailbox: Actor<ProductCollectionActorMsg>) =
  let rec loop() =
    actor {
      match! mailbox.Receive() with
      | Check q ->
        mailbox.Self <! Status "Check"
        let db = client.GetDatabase("Catalog")
        let collection = db.GetCollection<Product>("products")
        let count = collection.CountDocuments(Builders<Product>.Filter.Empty)
        if count = 0L then
          mailbox.Self <! GenerateAndStore q
        mailbox.Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromMinutes(1.), TimeSpan.FromMinutes(1.), mailbox.Self, Check(q), ActorRefs.NoSender)
        return! loop()
      | Status msg ->
        mailbox.Log.Value.Info(sprintf "Status(%A)" msg )
        return! loop()
      | GenerateAndStore q ->
        mailbox.Self <! Status "Start Generation"
        mailbox.Self <! DropCollection
        let p = generate(q)
        let products = p |> Seq.chunkBySize 500
        for ps in products do
          mailbox.Self <! Store(ps)
        mailbox.Self <! CreateIndexes
        return! loop()
      | Store p ->
        let db = client.GetDatabase("Catalog")
        let collection = db.GetCollection<Product>("products")
        async {
          mailbox.Self <! Status "Store Products Start"
          do! collection.InsertManyAsync(p) |> Async.AwaitTask
          return Status "Products Stored"
        } |!> mailbox.Self
        return! loop()
      | CreateIndexes ->
        let db = client.GetDatabase("Catalog")
        let collection = db.GetCollection<Product>("products")
        let tasks = seq { yield collection.Indexes.CreateOneAsync(CreateIndexModel<Product>(IndexKeysDefinition<Product>.op_Implicit(BsonDocument(dict [ "Slug" => 1 ])))) |> Async.AwaitTask
                          yield collection.Indexes.CreateOneAsync(CreateIndexModel<Product>(IndexKeysDefinition<Product>.op_Implicit(BsonDocument(dict [ "Name" => "text" ])))) |> Async.AwaitTask }
        async {
          do! tasks |> Async.Parallel |> Async.Ignore
          return Status "Indexes Created"
        } |!> mailbox.Self
        return! loop()
      | DropCollection ->
        let db = client.GetDatabase("Catalog")
        async {
         do! db.DropCollectionAsync("products") |> Async.AwaitTask
         return Status "Collection Droped"
        } |!> mailbox.Self
        return! loop()

    }
  loop()

[<EntryPoint>]
let main argv =
    let parser = ArgumentParser.Create<Arguments>(programName = "Catalog.Import")
    let results = parser.Parse(argv, raiseOnUsage = false, ignoreUnrecognized = true)
    if not results.IsUsageRequested then
      let system = ConfigurationFactory.Default() |> System.create "CatalogImport"
      let client = MongoClient(results.GetResult(ConnectionString,  defaultValue = "mongodb://127.0.0.1:27017"))
      let actor = spawn system "products" (ProductCollectionActor client)
      actor <! Check(results.GetResult(ProductsQuantity, defaultValue = 100))
      Console.ReadLine() |> ignore
    else
      parser.PrintUsage() |> printfn "%s"
    0 // return an integer exit code
