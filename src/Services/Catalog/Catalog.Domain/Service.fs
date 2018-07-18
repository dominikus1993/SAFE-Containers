namespace Catalog.Api.Services

open System.Threading.Tasks
open Catalog.Api.Model
open FSharp.Control.Tasks

module Product =

  let getBySlug (f: (string) -> Task<Product option> ) slug =
    task {
      return! f(slug)
    }
