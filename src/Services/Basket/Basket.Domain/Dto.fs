namespace Basket.Domain.Dto
open System
open Basket.Domain.Model.Aggregates
open Basket.Domain.Model.Entities

[<CLIMutable>]
type CustomerBasketItemDto = { ProductId: Guid; Quantity: int }

[<CLIMutable>]
type CustomerBasketDto = { Id: Guid; CustomerId: Guid; Items: CustomerBasketItemDto seq; CreationTime: DateTime; LastUpdate: DateTime }

module CustomerBasketItemDto =
  let fromDomain(domain: CustomerBasketItem) =
    { ProductId = domain.Id; Quantity = domain.Quantity }

  let toDomain(dto: CustomerBasketItemDto) =
    { Id = dto.ProductId; Quantity = dto.Quantity }

module CustomerBasketDto =
  let fromDomain(domain: CustomerBasket) =
    { Id = domain.Id; CustomerId = domain.CustomerData.Id; Items = domain.Items |> List.map(CustomerBasketItemDto.fromDomain) |> List.toSeq; CreationTime = domain.History.CreationTime; LastUpdate = domain.History.LastUpdate }

  let toDomain(dto: CustomerBasketDto): CustomerBasket =
    { Id = dto.Id; CustomerData = { Id = dto.CustomerId }; Items = dto.Items |> Seq.map(CustomerBasketItemDto.toDomain) |> Seq.toList; History = { CreationTime = dto.CreationTime; LastUpdate = dto.LastUpdate } }
