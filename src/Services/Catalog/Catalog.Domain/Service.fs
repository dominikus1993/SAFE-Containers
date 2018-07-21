namespace Catalog.Api.Services

open System.Threading.Tasks
open Catalog.Api.Model
open FSharp.Control.Tasks
open Catalog.Api.Repositories

module Product =

  let getBySlug (f: (string) -> Task<Result<Product, exn>> ) slug =
    task {
      return! f(slug)
    }
