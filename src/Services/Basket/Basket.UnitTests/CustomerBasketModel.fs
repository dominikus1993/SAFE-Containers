module CustomerBasketModel


open Expecto
open Basket.Domain.Model.Aggregates
open Basket.Domain.Model.Entities
open System

[<Tests>]
let tests =
  testList "CustomerBasket" [
    testList "AddItem" [
      testCase "Add item when basket is empty" <| fun _ ->
        let uuid = Guid.NewGuid()
        let subject = CustomerBasket.zero() |> CustomerBasket.addItem { Id = uuid; Quantity = 1 }
        Expect.equal subject.Items ([{ Id = uuid; Quantity = 1 } ]) "Item should contain [{ Id = uuid; Quantity = 1 } ]"
      testCase "Add item when basket is not empty" <| fun _ ->
        let uuid = Guid.NewGuid()
        let subject = CustomerBasket.zero() |> CustomerBasket.addItem { Id = uuid; Quantity = 1 }  |> CustomerBasket.addItem { Id = uuid; Quantity = 1 }
        Expect.equal subject.Items ([{ Id = uuid; Quantity = 2 } ]) "Item should contain [{ Id = uuid; Quantity = 2 } ]"
      testCase "Add two distinct items when basket is not empty" <| fun _ ->
        let uuid = Guid.NewGuid()
        let uuid2 = Guid.NewGuid()
        let subject = CustomerBasket.zero() |> CustomerBasket.addItem { Id = uuid; Quantity = 1 }  |> CustomerBasket.addItem { Id = uuid2; Quantity = 1 }
        Expect.equal subject.Items ([{ Id = uuid2; Quantity = 1 }; { Id = uuid; Quantity = 1 } ]) "Item should contain [{ Id = uuid; Quantity = 1 }; { Id = uuid2; Quantity = 1 } ]"
    ]
  ]
