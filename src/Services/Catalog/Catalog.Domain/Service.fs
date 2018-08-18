namespace Catalog.Api.Services

open System.Threading.Tasks
open Catalog.Api.Model
open FSharp.Control.Tasks
open Catalog.Api.Repositories

[<CLIMutable>]
type GetProducts =
  { pageSize : int option
    pageIndex : int option
    sort : string option
    priceMin : double option
    priceMax : double option
    name : string option
    tags : string option }

module Tags =
  let all (f : unit -> Task<Result<Tag seq, exn>>) : Task<Result<Tag seq, exn>> =
     task { return! f () }

module Product =
  let getBySlug (f : string -> Task<Result<Product, exn>>) slug = task { return! f (slug) }

  let get (f : BrowseProducts -> Task<Result<PagedProducts, exn>>) (req : GetProducts) : Task<Result<Data<Product seq, PagedMeta>, exn>> =
    task {
      let sortQ = defaultArg req.sort "default"
      let tagsArray = req.tags |> Option.bind(fun tags -> match tags.Split([|','|]) with | [||] -> None | tags -> Some(tags))
      let pageIndex = defaultArg req.pageIndex 1
      let pageSize = defaultArg req.pageSize 15
      let browse =
        { skip = (pageIndex - 1) * pageSize
          take = pageSize
          sort = sortQ
          priceMin = req.priceMin
          priceMax = req.priceMax
          name = req.name
          tags = tagsArray }
      let! result = f (browse)
      return result
             |> Result.bind (fun products ->
                  Ok({ Data = products.Products
                       Metadata =
                         { Page = pageIndex
                           TotalItems = products.TotalItems
                           TotalPages = products.TotalPages } }))
    }
