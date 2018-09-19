namespace Basket.Domain.Service


open System
open Basket.Domain.Model.Aggregates
module CustomerBasket =
  type GetCustomerBasket = Guid -> Async<Result<CustomerBasket, exn>>

  let get (getBasket: GetCustomerBasket) customerId =
    getBasket customerId
