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
        let userId = Guid.NewGuid()
        let subject = CustomerBasket.zero(userId) |> CustomerBasket.addItem { Id = uuid; Quantity = 1 }
        Expect.equal subject.State (Active( { Items = [{ Id = uuid; Quantity = 1 } ] } )) "Item should contain [{ Id = uuid; Quantity = 1 } ]"
      testCase "Add item when basket is not empty" <| fun _ ->
        let uuid = Guid.NewGuid()
        let userId = Guid.NewGuid()
        let subject =
          CustomerBasket.zero(userId) |> CustomerBasket.addItem { Id = uuid; Quantity = 1 }  |> CustomerBasket.addItem { Id = uuid; Quantity = 1 }
        Expect.equal subject.State (Active( { Items = [{ Id = uuid; Quantity = 2 } ]})) "Item should contain [{ Id = uuid; Quantity = 2 } ]"
      testCase "Add two distinct items when basket is not empty" <| fun _ ->
        let uuid = Guid.NewGuid()
        let uuid2 = Guid.NewGuid()
        let userId = Guid.NewGuid()
        let subject = CustomerBasket.zero(userId) |> CustomerBasket.addItem { Id = uuid; Quantity = 1 }  |> CustomerBasket.addItem { Id = uuid2; Quantity = 1 }
        Expect.equal subject.State ((Active( { Items = [{ Id = uuid2; Quantity = 1 }; { Id = uuid; Quantity = 1 } ] } ))) "Item should contain [{ Id = uuid; Quantity = 1 }; { Id = uuid2; Quantity = 1 } ]"
    ]
    testList "RemoveItem" [
      testCase "Remove item when basket is empty" <| fun _ ->
        let uuid = Guid.NewGuid()
        let userId = Guid.NewGuid()
        let subject = CustomerBasket.zero(userId) |> CustomerBasket.removeItem { Id = uuid; Quantity = 1 }
        Expect.equal subject.State (Empty(NoItems)) "State should be empty"
      testCase "Remove item when basket is not empty ad item not exists" <| fun _ ->
        let uuid = Guid.NewGuid()
        let notExistientUuid = Guid.NewGuid()
        let userId = Guid.NewGuid()
        let subject =
          CustomerBasket.zero(userId)
            |> CustomerBasket.addItem { Id = uuid; Quantity = 1 }
            |> CustomerBasket.addItem { Id = uuid; Quantity = 1 }
            |> CustomerBasket.removeItem { Id = notExistientUuid; Quantity = 10 }
        Expect.equal subject.State (Active( { Items = [{ Id = uuid; Quantity = 2 } ]})) "Item should contain [{ Id = uuid; Quantity = 2 } ]"
      testCase "Remove item when basket is not empty ad item exists" <| fun _ ->
        let uuid = Guid.NewGuid()
        let uuid2 = Guid.NewGuid()
        let userId = Guid.NewGuid()
        let subject = CustomerBasket.zero(userId)
                        |> CustomerBasket.addItem { Id = uuid; Quantity = 1 }
                        |> CustomerBasket.addItem { Id = uuid2; Quantity = 1 }
                        |> CustomerBasket.removeItem { Id = uuid2; Quantity = 1 }
        Expect.equal subject.State ((Active( { Items = [{ Id = uuid; Quantity = 1 } ] } ))) "Item should contain [{ Id = uuid; Quantity = 1 }; ]"
      testCase "Remove items from basket with possible quantity" <| fun _ ->
        let uuid = Guid.NewGuid()
        let uuid2 = Guid.NewGuid()
        let userId = Guid.NewGuid()
        let subject = CustomerBasket.zero(userId) |> CustomerBasket.addItem { Id = uuid; Quantity = 1 }  |> CustomerBasket.addItem { Id = uuid2; Quantity = 1 } |> CustomerBasket.removeItem { Id = uuid; Quantity = 1 } |> CustomerBasket.removeItem { Id = uuid2; Quantity = 1 }
        Expect.equal subject.State (Empty(NoItems)) "Item should contain [{ Id = uuid; Quantity = 1 }; ]"
    ]
  ]
