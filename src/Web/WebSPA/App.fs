module WebSPA

open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Elmish
open Fable.Helpers.React
open Fable.Helpers.React.Props

type Model =
    { x : int }

type Msg =
    | Increment
    | Decrement

let init () =
    { x = 0 }, Cmd.ofMsg Increment

let update msg model =
    match msg with
    | Increment when model.x < 3 ->
        { model with x = model.x + 1 }, Cmd.ofMsg Increment

    | Increment ->
        { model with x = model.x + 1 }, Cmd.ofMsg Decrement

    | Decrement when model.x > 0 ->
        { model with x = model.x - 1 }, Cmd.ofMsg Decrement

    | Decrement ->
        { model with x = model.x - 1 }, Cmd.ofMsg Increment

let view model _ =
  div [ClassName "container"] [
        h1 [ClassName "Test"][ str (sprintf "Value: %i" model.x)  ]
  ]

open Elmish.React
open Elmish.Debug
open Elmish.HMR

Program.mkProgram Catalog.State.init Catalog.State.update Catalog.View.view
#if DEBUG
|> Program.withDebugger
#endif
|> Program.withReact "elmish-app"
|> Program.run
