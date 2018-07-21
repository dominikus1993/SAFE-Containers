// Learn more about F# at http://fsharp.org

open System
open Argu

type Arguments =
    | [<AltCommandLine("-c")>] ConnectionString of connstr:string
    | [<AltCommandLine("-q")>] ProductsQuantity of q:int
with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | ProductsQuantity _ -> "Specify a products quantity to generation"
            | ConnectionString _ -> "Specify a mongodb connectionstring."

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    0 // return an integer exit code
