namespace Basket.Domain.Model

open Aggregates
module Messages =
  type DomainMessage =
    | ElementNotExistsInBasket

module Values =
  open System
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

  module CustomerBasketItem =
    let incrementQuantity q item =
      { item with Quantity =  item.Quantity + q }
    let decrementQuantity q item =
      { item with Quantity =  item.Quantity - q }

module Aggregates =
  open Values
  open Entities
  open System

  type NoItems = NoItems
  type EmptyBasketState = NoItems
  type ActiveBasketState = { Items: CustomerBasketItem list; }

  type Basket =
    | Empty of Id: Id * CustomerData: CustomerData * History: History * EmptyBasketState
    | LookingAround of Id: Guid * CustomerData: CustomerData * Items: ActiveBasketState * History: History

  type CustomerBasket = { Id: Id; CustomerData: CustomerData; Basket: Basket; History: History }

  module CustomerBasket =
    let zero (customerId) =
      { Id = Guid.NewGuid(); CustomerData = { Id = customerId }; Basket = Empty(NoItems) ; History = { CreationTime = DateTime.UtcNow; LastUpdate = DateTime.UtcNow } }




