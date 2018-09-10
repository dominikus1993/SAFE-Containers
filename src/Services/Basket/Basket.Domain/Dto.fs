namespace Basket.Domain.Dto
open System
open Basket.Domain.Model.Aggregates

[<CLIMutable>]
type CustomerBasketItemDto = { ProductId: Guid; Quantity: int }

[<CLIMutable>]
type CustomerBasketDto = { Id: Guid; Items: CustomerBasketItemDto seq }

module CustomerBasket =
  let fromDomain(domain: CustomerBasket) =
    { Id = domain.Id; Items = domain.Items |> List.map(fun item -> { ProductId = item.Id; Quantity = item.Quantity}) |> List.toSeq }
