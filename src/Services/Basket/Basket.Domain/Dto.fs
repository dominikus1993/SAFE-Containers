namespace Basket.Domain.Dto
open System

[<CLIMutable>]
type CustomerBasketItemDto = { ProductId: int; Quantity: int }

[<CLIMutable>]
type CustomerBasketResponse = { Id: Guid; Items: CustomerBasketItemDto seq }
