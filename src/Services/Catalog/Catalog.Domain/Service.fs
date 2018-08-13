namespace Catalog.Api.Services

open System.Threading.Tasks
open Catalog.Api.Model
open FSharp.Control.Tasks
open Catalog.Api.Repositories

[<CLIMutable>]
type GetProducts = { pageSize: int; pageIndex: int; sort: string option; priceMin: double option; priceMax: double option; name: string option }

module Product =

  let getBySlug (f: (string) -> Task<Result<Product, exn>> ) slug =
    task {
      return! f(slug)
    }

  let get (f: (BrowseProducts) -> Task<Result<Product seq, exn>> ) (req: GetProducts) =
    task {
      let sortQ =  defaultArg req.sort "default"
      return! f({ skip = (req.pageIndex - 1) * req.pageSize; take = req.pageSize; sort = sortQ; priceMin = req.priceMin; priceMax = req.priceMax; name = req.name})
    }
