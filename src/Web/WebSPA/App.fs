module WebSPA

open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Elmish

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

let view =

Program.mkProgram init update (fun model _ -> printf "%A\n" model)
|> Program.withConsoleTrace
|> Program.run
