namespace Basket.Domain.Model

open System
module Values =

  type Id = Guid
  type History = { CreationTime: DateTime; LastUpdate: DateTime }
  type State =
    | New
    | LookingAround
    | Shopping
    | Paid

module Entities =
  open Values

  type CustomerBasketItem = { Id: Id; Quantity: int }

  type CustomerData = { Id: Id }

module Aggregates =
  open Values
  open Entities

  type CustomerBasket = { Id: Id; CustomerData: CustomerData; Items: CustomerBasketItem list; History: History }

  module CustomerBasket =
    let zero (customerId) =
      { Id = Guid.NewGuid(); CustomerData = { Id = customerId }; Items = []; History = { CreationTime = DateTime.UtcNow; LastUpdate = DateTime.UtcNow } }

    let addItem (item: CustomerBasketItem) (basket: CustomerBasket) =
      match basket.Items with
      | [] -> { basket with Items = [item]; History = { basket.History with LastUpdate = DateTime.UtcNow } }
      | list when list |> List.exists(fun x -> x.Id = item.Id ) ->
        { basket with Items = list |> List.map(fun i -> if i.Id = item.Id then { i with Quantity = i.Quantity + item.Quantity } else i ) ; History = { basket.History with LastUpdate = DateTime.UtcNow } }
      | list->
        { basket with Items = item :: list ; History = { basket.History with LastUpdate = DateTime.UtcNow } }



