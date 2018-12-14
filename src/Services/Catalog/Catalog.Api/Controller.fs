namespace Catalog.Api.Controllers
open FSharp.Control.Tasks
open Microsoft.AspNetCore.Http
open Giraffe
open Catalog.Api.Services
open Catalog.Api.Model
open Catalog.Api.Repositories
open Giraffe.HttpStatusCodeHandlers

module Tags =
  let private indexAction =
    fun (next : HttpFunc) (ctx: HttpContext) ->
      task {
        let repo = ctx.GetService<ITagsRepository>()
        match! Tags.all (repo.All) with
        | Ok (data) ->
           return! Successful.OK data next ctx
        | Error er ->
          return! Successful.NO_CONTENT next ctx
      }

  let controller: HttpFunc -> HttpContext -> HttpFuncResult =
    choose [
      GET >=>
        choose [
          route  "/" >=> indexAction
        ]]

module Products =

  let private showAction slug =
    fun (next : HttpFunc) (ctx: HttpContext) ->
      task {
        let repo = ctx.GetService<IProductRepository>()
        match! Product.getBySlug (repo.GetBySlug) slug with
        | Ok (data) ->
           return! Successful.OK data next ctx
        | Error er ->
          return! RequestErrors.notFound (text er.Message) next ctx
      }

  let private indexAction =
    fun (next : HttpFunc) (ctx: HttpContext) ->
      task {
        let repo = ctx.GetService<IProductRepository>()
        let queryS = ctx.BindQueryString<GetProducts>()
        match! Product.get (repo.Browse) queryS with
        | Ok (data) ->
           return! Successful.OK data next ctx
        | Error er ->
          return! RequestErrors.notFound (text er.Message) next ctx
      }


  let controller: HttpFunc -> HttpContext -> HttpFuncResult =
    choose [
      GET >=>
        choose [
          route  "/" >=> indexAction
          routef "/%s" showAction
        ]]
