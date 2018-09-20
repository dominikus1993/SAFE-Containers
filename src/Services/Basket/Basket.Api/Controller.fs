module Basket.Api.Controller
open Basket.Domain.Storage
open Basket.Domain.Model.Aggregates
open Basket.Domain.Dto
open System
open System.Security.Claims
open Basket.Domain.Service

module CustomerBasket =
  open Saturn
  open FSharp.Control.Tasks.V2.ContextInsensitive
  open Microsoft.AspNetCore.Http
  open Giraffe

  let private indexAction =
    fun (ctx : HttpContext) ->
      task {
        let userId = ctx.User.FindFirst ClaimTypes.NameIdentifier
        let repo =  ctx.GetService<ICustomerBasketRepository>()
        match! CustomerBasket.get repo.Get (Guid.Parse(userId.Value)) |> Async.StartAsTask with
        | Ok data ->
          return! Response.ok ctx (data)
        | Error err ->
          return! Response.internalError ctx err.Message
      }

  let controller = controller {
    index indexAction
  }
