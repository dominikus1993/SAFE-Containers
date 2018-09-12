namespace Basket.Domain.Model

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

  type CustomerBasket = { Id: Id; CustomerData: CustomerData; Items: CustomerBasketItem list; History: History }

  type Basket =
    | Empty of Id: Guid
    |

  module CustomerBasket =
    let zero (customerId) =
      { Id = Guid.NewGuid(); CustomerData = { Id = customerId }; Items = []; History = { CreationTime = DateTime.UtcNow; LastUpdate = DateTime.UtcNow } }

    let addItem (item: CustomerBasketItem) (basket: CustomerBasket): Result<CustomerBasket, Messages.DomainMessage> =
      match basket.Items with
      | [] -> Ok({ basket with Items = [item]; History = { basket.History with LastUpdate = DateTime.UtcNow } })
      | list when list |> List.exists(fun x -> x.Id = item.Id ) ->
        Ok({ basket with Items = list |> List.map(fun i -> if i.Id = item.Id then i |> CustomerBasketItem.incrementQuantity item.Quantity else i ) ; History = { basket.History with LastUpdate = DateTime.UtcNow } })
      | list->
        Ok({ basket with Items = item :: list ; History = { basket.History with LastUpdate = DateTime.UtcNow } })




