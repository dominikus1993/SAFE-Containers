
module Fisher.Tests
open Expecto
open Order.Domain.Aggregates
open System

[<Tests>]
let testOrder =
  testList "Order" [
    testList "Create" [
      test "Create" {
        let id = Guid.NewGuid()
        let subject = Order.Create(id)
        Expect.equal subject { Id = id } "OrderAggregate should eq"
      }
    ]
  ]
