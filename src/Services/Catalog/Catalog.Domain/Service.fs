namespace Catalog.Api.Services

open System.Threading.Tasks
open Catalog.Api.Model
open FSharp.Control.Tasks
open Catalog.Api.Repositories

[<CLIMutable>]
type GetProducts = { pageSize: int; pageIndex: int }

module Product =

  let getBySlug (f: (string) -> Task<Result<Product, exn>> ) slug =
    task {
      return! f(slug)
    }

  let get (f: (BrowseProducts) -> Task<Result<Product seq, exn>> ) (req: GetProducts) =
    task {
      return! f({ skip = (req.pageIndex - 1) * req.pageSize; take = req.pageSize })
    }
