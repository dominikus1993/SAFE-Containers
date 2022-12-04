namespace Basket.Domain.Model

module Messages =
  type DomainMessage =
    | ElementNotExistsInBasket

module Values =
  open System
  type Id = Guid
  [<Struct>]
  type History = { CreationTime: DateTime; LastUpdate: DateTime }

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

  type EmptyState = NoItems
  type ActiveState = { Items: CustomerBasketItem list }

  type BasketState =
    | Empty of EmptyState
    | Active of ActiveState

  type CustomerBasket = { Id: Id; CustomerData: CustomerData; State: BasketState ; History: History }

  module CustomerBasket =
    let zero id customerId =
      { Id = id; CustomerData = { Id = customerId }; State = Empty(NoItems); History = { CreationTime = DateTime.UtcNow; LastUpdate = DateTime.UtcNow } }

    let addItem (item: CustomerBasketItem) (basket: CustomerBasket) =
      match basket.State with
      | Empty(_) -> { basket with State = Active({ Items = [item] }); History = { basket.History with LastUpdate = DateTime.UtcNow } }
      | Active(state)->
        match state with
        | { Items = list } when list |> List.exists(fun i -> i.Id = item.Id) ->
          let newItemList = list |> List.map(fun i -> if i.Id = item.Id then { i with Quantity = i.Quantity + item.Quantity } else i )
          { basket with State = Active({ Items = newItemList }); History = { basket.History with LastUpdate = DateTime.UtcNow } }
        | { Items = list } ->
          { basket with State = Active({ Items = item :: list  }); History = { basket.History with LastUpdate = DateTime.UtcNow } }

    let removeItem(item: CustomerBasketItem) (basket: CustomerBasket) =
      match basket.State with
      | Empty(_) -> basket
      | Active(state) ->
        let items = state.Items
                      |> List.map(fun i -> if i.Id = item.Id then i |> CustomerBasketItem.decrementQuantity item.Quantity else i)
                      |> List.filter(fun i -> i.Quantity > 0)
        match items with
        | [] -> { basket with State = Empty(NoItems); History = { basket.History with LastUpdate = DateTime.UtcNow} }
        | items -> { basket with State = Active({ Items = items }); History = { basket.History with LastUpdate = DateTime.UtcNow}}

    let clear(basket: CustomerBasket) =
      { basket with State = Empty(NoItems);  History = { basket.History with LastUpdate = DateTime.UtcNow} }





