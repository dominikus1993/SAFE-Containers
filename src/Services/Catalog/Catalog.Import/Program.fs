// Learn more about F# at http://fsharp.org

open System
open Argu
open Bogus
open Catalog.Api.Model

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
  Faker<Product>()
    .CustomInstantiator(fun f ->
                          { Id = Guid.NewGuid()
                            Slug = f.Commerce.Product()
                            Name = f.Commerce.ProductName()
                            Description = f.Commerce.ProductAdjective()
                            Details = { Weight = f.Random.Double(); WeightUnits = "kg"; Manufacturer = f.Company.CompanyName(); Color = f.Internet.Color() }
                            Price = f.Commerce.Price() |> Decimal.Parse
                            AvailableStock = f.Random.Number()
                            PictureUri = f.Image.Food()
                            Tags = [| f.Lorem.Word(); f.Lorem.Word(); f.Lorem.Word();|]}
                        )

[<EntryPoint>]
let main argv =

    printfn "Hello World from F#!"
    0 // return an integer exit code
