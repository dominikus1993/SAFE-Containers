namespace Basket.Domain.Model

open System
module Values =

  type Id = Guid

module Entities =
  open Values

  type CustomerBasketItem = { Id: Id; Quantity: int }

module Aggregates =
  open Values
  open Entities

  type CustomerBasket = { Id: Id; Items: CustomerBasketItem list; LastUpdate: DateTime; CreationTime: DateTime }
