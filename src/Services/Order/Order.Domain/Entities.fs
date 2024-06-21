namespace Order.Domain.Entities
open Order.Domain.Values

type Customer = { Id: CustomerId }

type Item = { Id: ItemId; Name: string; UnitPrice: decimal; Discount: decimal option; Units: int }
