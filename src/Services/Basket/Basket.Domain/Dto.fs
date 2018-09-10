namespace Basket.Domain.Dto
open System
open Basket.Domain.Model.Aggregates
open Basket.Domain.Model.Entities

[<CLIMutable>]
type CustomerBasketItemDto = { ProductId: Guid; Quantity: int }

[<CLIMutable>]
type CustomerBasketDto = { Id: Guid; Items: CustomerBasketItemDto seq; CreationTime: DateTime; LastUpdate: DateTime }

module CustomerBasketDto =
  let fromDomain(domain: CustomerBasket) =
    { Id = domain.Id; Items = domain.Items |> List.map(fun item -> { ProductId = item.Id; Quantity = item.Quantity}) |> List.toSeq; CreationTime = domain.History.CreationTime; LastUpdate = domain.History.LastUpdate }

  let toDomain(dto: CustomerBasketDto): CustomerBasket =
    { Id = dto.Id; Items = dto.Items |> Seq.map(fun item -> { Id = item.ProductId; Quantity = item.Quantity }) |> Seq.toList; History = { CreationTime = dto.CreationTime; LastUpdate = dto.LastUpdate } }
