namespace Basket.Domain.Service


open System
open Basket.Domain.Dto
open Basket.Domain.Messages
open Basket.Domain.Model.Aggregates
open Basket.Domain.Storage

module CustomerBasket =
  type GetCustomerBasket = Guid -> Async<Result<CustomerBasketDto, ErrorMessage>>
  type DeleteCustomerBasket = CustomerBasketDto -> Async<Result<CustomerBasketDto, ErrorMessage>>
  type InsertCustomerBasket = CustomerBasketDto -> Async<Result<CustomerBasketDto, ErrorMessage>>
  type UpdateCustomerBasket = CustomerBasketDto -> Async<Result<CustomerBasketDto, ErrorMessage>>

  let get (getBasket: GetCustomerBasket) customerId =
    getBasket customerId

  let remove (deleteBasket: DeleteCustomerBasket) customerBasket =
    deleteBasket customerBasket

  let addItem (getBasket: GetCustomerBasket) (insertBasket: InsertCustomerBasket)(updateBasket: UpdateCustomerBasket)(basketId, customerId)(customerBasketItem) =
    async {
      match! getBasket customerId with
      | Ok basket ->
        let newBasket = basket
                        |> CustomerBasketDto.toDomain
                        |> CustomerBasket.addItem (customerBasketItem |> CustomerBasketItemDto.toDomain)
                        |> CustomerBasketDto.fromDomain
        return! updateBasket(newBasket)
      | Error err ->
        match err with
        | BasketNotExists ->
          let basket = CustomerBasket.zero basketId customerId
                        |> CustomerBasket.addItem (customerBasketItem |> CustomerBasketItemDto.toDomain)
                        |> CustomerBasketDto.fromDomain
          return! insertBasket(basket)
    }

  let removeItem (getBasket: GetCustomerBasket)(updateBasket: UpdateCustomerBasket)(basketId, customerId)(customerBasketItem) =
    getBasket customerId
      |> AsyncResult.bind(fun basket ->
                            async {
                              let newBasket = basket
                                                |> CustomerBasketDto.toDomain
                                                |> CustomerBasket.removeItem (customerBasketItem |> CustomerBasketItemDto.toDomain)
                                                |> CustomerBasketDto.fromDomain
                              return! updateBasket(newBasket)
                            })


