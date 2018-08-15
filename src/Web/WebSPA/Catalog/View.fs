module Catalog.View
open Elmish
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Catalog.Types
open Catalog.State
open Fulma
open Fulma

let productComponent(p: Product) =
  Card.card [] [
    Card.image [] [
      Image.image [ Image.Is128x128 ] [
        img [ Src p.pictureUri ]
      ]
    ]
    Card.content [] [
      Media.media [] [
        Media.content [][
          Tile.tile [Fulma.Tile.Option.Size Fulma.Tile.Is4] [
            str p.name
          ]
          Tile.tile [Fulma.Tile.Option.Size Fulma.Tile.Is6 ] [
            str (sprintf " %.2f" p.price)
          ]
        ]
      ]
    ]
  ]

let view model dispatch =
  div [ClassName "container"] [
        h1 [] [ str (sprintf "TotalPages: %d" model.TotalPages) ]
        ul [] (model.Products |> Array.map(fun p ->  li [Key p.id] [ (productComponent p) ]) |> Array.toList)
  ]
