// Learn more about F# at http://fsharp.org

open System
open StackExchange.Redis
open Basket.Domain.Storage
open Basket.Domain.Model.Aggregates
open Basket.Domain.Dto
open Basket.Domain.Dto

[<EntryPoint>]
let main argv =
    let multiplexer = ConnectionMultiplexer.Connect("localhost")
    let repo = CustomerBasket.storage multiplexer
    let userID = Guid.Parse("823f9603-00fe-4329-b091-997a25dc2f8f")
    printfn "%A" userID
    // let basket = CustomerBasket.zero(userID) |> CustomerBasket.addItem { Id = Guid.NewGuid(); Quantity = 2 }
    // let insertTask = repo.Insert (basket |> CustomerBasketDto.fromDomain)
    // insertTask.Wait()
    let basket = repo.Get userID |> Async.RunSynchronously //|> CustomerBasketDto.toDomain
    match basket with
    | Ok data ->
      let rmRes = repo.Remove data |> Async.RunSynchronously
      printfn "%A" rmRes
    printfn "Hello World from F#!"
    0 // return an integer exit code
