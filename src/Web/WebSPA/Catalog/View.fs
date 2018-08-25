module Catalog.View
open Elmish
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Catalog.Types
open Catalog.State
open Fulma

let tagComponent(tag: Tag) =
    Label.label [] [
      Checkbox.checkbox [] [
        Checkbox.input []
        str (sprintf "%s(%d)" tag.name tag.quantity)
      ]
    ]

let tagListComponent (tags: Tag array) =
  Field.div [] [
    ul [] (tags |> Array.map(fun tag -> li [Key tag.name] [ tagComponent tag ] ) |> Array.toList)
  ]

let productTagsComponent (tags: string array) =
  Tag.list [] (tags |> Array.map(fun tag -> Tag.tag [] [ str tag ] ) |> Array.toList )

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
          p.tags |> productTagsComponent
        ]
      ]
    ]
  ]

let productListComponent products =
  div [] [
        ul [] (products |> Array.map(fun p ->  li [Key p.id] [ (productComponent p) ]) |> Array.toList)
  ]

let view model dispatch =
  div [ClassName "container"] [
    h1 [] [ str (sprintf "TotalPages: %d" model.TotalPages) ]
    productListComponent model.Products
    tagListComponent model.Tags
  ]

