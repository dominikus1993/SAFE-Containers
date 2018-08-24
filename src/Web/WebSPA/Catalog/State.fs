module Catalog.State

open Catalog.Types
open Fable.Helpers.React.ReactiveComponents
open Elmish
open Fable.PowerPack
open System.Net.Http.Headers
open Fable.PowerPack.Fetch
open Fable.Core

[<Pojo>]
type ProductsQuery =
  { page : int
    pageSize : int
    sort : string
    filters : ProductFilter array }

let addFiltersToQueryString (filters : ProductFilter list) url =
  let rec func (f : ProductFilter list) (acc : string) =
    match f with
    | [] -> acc
    | head :: tail ->
      match head with
      | PriceMin p -> func tail (acc + (sprintf "&priceMin=%.2f" p))
      | PriceMax p -> func tail (acc + (sprintf "&priceMax=%.2f" p))
      | Name name -> func tail (acc + (sprintf "&name=%s" name))
  url + (func filters "")

let getProducts (query : ProductsQuery) =
  promise {
    let url =
      sprintf "%s%s" Urls.CatalogApiUrl
        (sprintf "products?pageSize=%d&pageIndex=%d&sort=%s" query.pageSize query.page query.sort)
      |> addFiltersToQueryString (query.filters |> Array.toList)
    let props = [ RequestProperties.Method HttpMethod.GET ]
    return! fetchAs<ProductData<Product array, PagedMeta>> url props
  }


let getProductsCmd (query : ProductsQuery) = Cmd.ofPromise getProducts query FetchedProducts FetchError

let getTags () =
  promise {
    let url =
      sprintf "%s%s" Urls.CatalogApiUrl
        (sprintf "tags")
    let props = [ RequestProperties.Method HttpMethod.GET ]
    return! fetchAs<Tag array> url props
  }


let getTagsCmd () = Cmd.ofPromise getTags () FetchedTags FetchError

let initCmd (): Cmd<Msg> =
  let getproducts = getProductsCmd { page = 1
                                     pageSize = 15
                                     sort = "priceAsc"
                                     filters = [| PriceMin(15m) |] }
  let getTags = getTagsCmd()
  Cmd.batch [getproducts; getTags]

let init() : Model * Cmd<Msg> =
  { Products = [||]
    Tags = [||]
    Page = 1
    PageSize = 15
    TotalItems = 0
    TotalPages = 0
    ErrorMessage = None
    Loading = false
    Sort = "default" },
   initCmd()

let update msg model =
  match msg with
  | BrowseProducts(page, pageSize, sort, filters) ->
    let query : ProductsQuery =
      { page = page
        pageSize = pageSize
        sort = sort
        filters = filters }
    { model with Loading = true }, getProductsCmd (query)
  | GetTags ->
      { model with Loading = true }, getTagsCmd ()
  | FetchedTags tags ->
    { model with Tags = tags; Loading = false}, Cmd.none
  | FetchedProducts(res) ->
    { model with Products = res.data
                 TotalItems = res.metadata.totalItems
                 Page = res.metadata.page
                 TotalPages = res.metadata.totalPages
                 Loading = false }, Cmd.none
  | FetchError err -> { model with ErrorMessage = Some(err.Message) }, Cmd.none
