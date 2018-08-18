namespace Catalog.Api.Controllers
open Saturn
open FSharp.Control.Tasks
open Microsoft.AspNetCore.Http
open Giraffe
open Catalog.Api.Services
open Catalog.Api.Model
open Catalog.Api.Repositories

module Tags =
  let private indexAction =
    fun (ctx : HttpContext) ->
      task {
        let repo = ctx.GetService<ITagsRepository>()
        match! Tags.all (repo.All) with
        | Ok (data) ->
           return! Response.ok ctx data
        | Error er ->
          return! Response.notFound ctx er.Message
      }

  let controller = controller {
    index indexAction
  }

module Products =

  let private showAction =
    fun (ctx : HttpContext) slug ->
      task {
        let repo = ctx.GetService<IProductRepository>()
        match! Product.getBySlug (repo.GetBySlug) slug with
        | Ok (data) ->
           return! Response.ok ctx data
        | Error er ->
          return! Response.notFound ctx er.Message
      }

  let private indexAction =
    fun (ctx : HttpContext) ->
      task {
        let repo = ctx.GetService<IProductRepository>()
        let queryS = Controller.getQuery<GetProducts> ctx
        match! Product.get (repo.Browse) queryS with
        | Ok (data) ->
           return! Response.ok ctx data
        | Error er ->
          return! Response.notFound ctx er.Message
      }

  let controller = controller {
    index indexAction
    show showAction
  }
