module Catalog.View
open Elmish
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Catalog.Types
open Catalog.State

let view model dispatch =
  div [ClassName "container"] [
        h1 [ClassName "Test"][ str (sprintf "Value: %A" model.Products)  ]
        button [ OnClick (fun _ -> dispatch(BrowseProducts(1, 19)))] [ str "Click Me"]
  ]
