module Basket.Api.Controller
open Basket.Domain.Storage
open Basket.Domain.Model.Aggregates
open Basket.Domain.Dto
open System
open System.Security.Claims

module CustomerBasket =
  open Saturn
  open FSharp.Control.Tasks.V2.ContextInsensitive
  open Microsoft.AspNetCore.Http
  open Giraffe

  let private indexAction =
    fun (ctx : HttpContext) ->
      task {
        let userId = ctx.User.FindFirst ClaimTypes.NameIdentifier
        return! Response.ok ctx (userId.Value)
      }

  let controller = controller {
    index indexAction
  }
