namespace Catalog.Api.Services

open System.Threading.Tasks
open Catalog.Api.Model
open FSharp.Control.Tasks
open Catalog.Api.Repositories

[<CLIMutable>]
type GetProducts =
  { pageSize : int
    pageIndex : int
    sort : string option
    priceMin : double option
    priceMax : double option
    name : string option
    tags : string array option }

module Product =
  let getBySlug (f : string -> Task<Result<Product, exn>>) slug = task { return! f (slug) }

  let get (f : BrowseProducts -> Task<Result<PagedProducts, exn>>) (req : GetProducts) : Task<Result<Data<Product seq, PagedMeta>, exn>> =
    task {
      let sortQ = defaultArg req.sort "default"

      let browse =
        { skip = (req.pageIndex - 1) * req.pageSize
          take = req.pageSize
          sort = sortQ
          priceMin = req.priceMin
          priceMax = req.priceMax
          name = req.name
          tags = req.tags }
      let! result = f (browse)
      return result
             |> Result.bind (fun products ->
                  Ok({ Data = products.Products
                       Metadata =
                         { Page = req.pageIndex
                           TotalItems = products.TotalItems
                           TotalPages = products.TotalPages } }))
    }
