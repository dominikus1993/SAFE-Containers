namespace Catalog.Api.Controllers
open Saturn
open FSharp.Control.Tasks
open Microsoft.AspNetCore.Http
open Giraffe
open Microsoft.Extensions.Options
open Saturn.ControllerHelpers
open Saturn
open MongoDB.Driver
open Catalog.Api.Services
open Catalog.Api.Model
open Catalog.Api.Repositories
open Microsoft.AspNetCore.Mvc


module Products =

  let showAction =
    fun (ctx : HttpContext) slug ->
      task {
        let repo = ctx.GetService<IProductRepository>()
        let! result = Product.getBySlug (repo.GetBySlug) slug
        match result with
        | Ok (data) ->
           return! Response.ok ctx data
        | Error er ->
          return! Response.notFound ctx er.Message
      }

  let indexAction =
    fun (ctx : HttpContext) ->
      task {
        let repo = ctx.GetService<IProductRepository>()
        let queryS = Controller.getQuery<GetProducts> ctx
        let! result = Product.get (repo.Get) queryS
        match result with
        | Ok (data) ->
           return! Response.ok ctx data
        | Error er ->
          return! Response.notFound ctx er.Message
      }

  let controller = controller {
    index indexAction
    show showAction
  }
