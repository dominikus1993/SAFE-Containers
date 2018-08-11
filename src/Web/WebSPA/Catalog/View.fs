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
      Image.image [ Image.Is3by4 ] [
        img [ Src p.pictureUri ]
      ]
    ]
    Card.content [] [
      Media.media [] [
        Media.content [][
          Tile.tile [Fulma.Tile.Option.Size Fulma.Tile.Is4] [
            str p.name
          ]
        ]
      ]
    ]
  ]

let view model dispatch =
  div [ClassName "container"] [
        ul [] (model.Products |> Array.map(fun p ->  li [Key p.id] [ (productComponent p) ]) |> Array.toList)
  ]
