namespace Basket.Domain.Model

open System
module Values =

  type Id = Guid
  type History = { CreationTime: DateTime; LastUpdate: DateTime }

module Entities =
  open Values

  type CustomerBasketItem = { Id: Id; Quantity: int }

module Aggregates =
  open Values
  open Entities

  type CustomerBasket = { Id: Id; Items: CustomerBasketItem list; History: History } with
    member this.AddItem(item: CustomerBasketItem) =
      this
