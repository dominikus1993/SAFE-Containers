module Catalog.State
open Catalog.Types
open Fable.Helpers.React.ReactiveComponents
open Elmish
open Fable.PowerPack
open System.Net.Http.Headers
open Fable.PowerPack.Fetch


type ProductsQuery = { Page: int; PageSize: int32 }

let getProducts(query: ProductsQuery) =
  promise {
    let url = sprintf "%s%s" Urls.CatalogApiUrl (sprintf "products?pageSize=%d&pageIndex=%d" query.PageSize query.Page)
    let props =
      [ RequestProperties.Method HttpMethod.GET ]
    return! fetchAs<Product array> url props
  }

let getProductsCmd(query: ProductsQuery) =
  Cmd.ofPromise getProducts query FetchedProducts FetchError

let init () : Model * Cmd<Msg> =
  { Products = [||]; Page = 1; PageSize = 15; ErrorMessage = None; Loading = false }, getProductsCmd { Page = 1; PageSize = 15 }

let update msg model =
  match msg with
  | BrowseProducts(page, pageSize)->
    let query: ProductsQuery = { Page = page; PageSize = pageSize }
    { model with Loading = true }, getProductsCmd(query)
  | FetchedProducts(products) ->
    { model with Products = products; Loading = false }, Cmd.none
  | FetchError err ->
    { model with ErrorMessage = Some(err.Message)}, Cmd.none
