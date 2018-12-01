namespace Order.Domain.Aggregates
open Order.Domain.Values

type OrderAggregate = { Id: OrderId } with
  static member Create(id: OrderId) =
    { Id = id }
