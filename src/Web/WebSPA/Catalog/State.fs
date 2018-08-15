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
    return! fetchAs<ProductData<Product array, PagedMeta>> url props
  }

let getProductsCmd(query: ProductsQuery) =
  Cmd.ofPromise getProducts query FetchedProducts FetchError

let init () : Model * Cmd<Msg> =
  { Products = [||]; Page = 1; PageSize = 15; TotalItems = 0; TotalPages = 0; ErrorMessage = None; Loading = false; Sort = "default" }, getProductsCmd { Page = 1; PageSize = 15; sort = "default" }

let update msg model =
  match msg with
  | BrowseProducts(page, pageSize, sort, filters)->
    let query: ProductsQuery = { Page = page; PageSize = pageSize; sort = sort  }
    { model with Loading = true }, getProductsCmd(query)
  | FetchedProducts(res) ->
    { model with Products = res.data; TotalItems = res.metadata.totalItems; Page = res.metadata.page; TotalPages = res.metadata.totalPages; Loading = false }, Cmd.none
  | FetchError err ->
    { model with ErrorMessage = Some(err.Message)}, Cmd.none
