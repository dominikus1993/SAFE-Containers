module Catalog.State
open Catalog.Types
open Fable.Helpers.React.ReactiveComponents
open Elmish
open Fable.PowerPack
open System.Net.Http.Headers
open Fable.PowerPack.Fetch
open Fable.Core

[<Pojo>]
type ProductsQuery = { Page: int; PageSize: int; sort: string }

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
  { Products = [||]; Page = 1; PageSize = 15; ErrorMessage = None; Loading = false; Sort = "default" }, getProductsCmd { Page = 1; PageSize = 15; sort = "default" }

let update msg model =
  match msg with
  | BrowseProducts(page, pageSize, sort)->
    let query: ProductsQuery = { Page = page; PageSize = pageSize; sort = sort  }
    { model with Loading = true }, getProductsCmd(query)
  | FetchedProducts(products) ->
    { model with Products = products; Loading = false }, Cmd.none
  | FetchError err ->
    { model with ErrorMessage = Some(err.Message)}, Cmd.none
