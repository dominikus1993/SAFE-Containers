module CustomerBasketModel


open Expecto

[<Tests>]
let tests =
  testList "CustomerBasket" [
    testList "AddItem" [
      testCase "Add item when basket is empty" <| fun _ ->
        Expect.isTrue true ""
    ]
  ]
