namespace Order.Domain.Aggregates
open Order.Domain.Values
open Order.Domain.Entities

type Order = { Id: OrderId; } with
  static member Create(id: OrderId) =
    { Id = id }
