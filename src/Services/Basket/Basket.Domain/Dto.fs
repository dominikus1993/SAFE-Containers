namespace Basket.Domain.Dto
open System
open Basket.Domain.Model.Aggregates
open Basket.Domain.Model.Entities
open System

[<CLIMutable>]
type CustomerBasketItemDto = { ProductId: Guid; Quantity: int }

[<CLIMutable>]
type CustomerBasketDto = { Id: Guid; CustomerId: Guid; Items: CustomerBasketItemDto seq; CreationTime: DateTime; LastUpdate: DateTime }

[<CLIMutable>]
type CustomerBasketResponseDto = { Id: Guid; Items: CustomerBasketItemDto seq; }

module CustomerBasketItemDto =
  let fromDomain(domain: CustomerBasketItem) =
    { ProductId = domain.Id; Quantity = domain.Quantity }

  let toDomain(dto: CustomerBasketItemDto) =
    { Id = dto.ProductId; Quantity = dto.Quantity }

module CustomerBasketResponseDto =
  let zero() = { Id = Guid.NewGuid(); Items = []; }

  let fromDomain(domain: CustomerBasket) =
    let items = match domain.State with
                | Empty(_) -> []
                | Active(state) -> state.Items
    { Id = domain.Id; Items = items |> List.map(CustomerBasketItemDto.fromDomain) |> List.toSeq; }

module CustomerBasketDto =
  let zero userId = { Id = Guid.NewGuid(); CustomerId = userId; Items = []; CreationTime = DateTime.UtcNow; LastUpdate = DateTime.UtcNow }

  let fromDomain(domain: CustomerBasket) =
    let items = match domain.State with
                | Empty(_) -> []
                | Active(state) -> state.Items
    { Id = domain.Id; CustomerId = domain.CustomerData.Id; Items = items |> List.map(CustomerBasketItemDto.fromDomain) |> List.toSeq; CreationTime = domain.History.CreationTime; LastUpdate = domain.History.LastUpdate }

  let toDomain(dto: CustomerBasketDto): CustomerBasket =
    let state = if dto.Items |> Seq.isEmpty then Empty(NoItems) else Active({ Items = dto.Items |> Seq.map(CustomerBasketItemDto.toDomain) |> Seq.toList })
    { Id = dto.Id; CustomerData = { Id = dto.CustomerId }; State = state; History = { CreationTime = dto.CreationTime; LastUpdate = dto.LastUpdate } }
