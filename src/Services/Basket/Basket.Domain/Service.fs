namespace Basket.Domain.Service


open System
open Basket.Domain.Model.Aggregates
open Basket.Domain.Dto

module CustomerBasket =
  type GetCustomerBasket = Guid -> Async<Result<CustomerBasketDto, exn>>

  let get (getBasket: GetCustomerBasket) customerId =
    getBasket customerId
