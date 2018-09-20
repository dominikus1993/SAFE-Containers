module Basket.Api.Controller
open Basket.Domain.Storage
open Basket.Domain.Model.Aggregates
open Basket.Domain.Dto
open System

module CustomerBasket =
  open Saturn
  open FSharp.Control.Tasks.V2.ContextInsensitive
  open Microsoft.AspNetCore.Http
  open Giraffe

  let private indexAction =
    fun (ctx : HttpContext) ->
      task {
        return! Response.ok ctx (CustomerBasket.zero(Guid.NewGuid()) |> CustomerBasketResponseDto.fromDomain)
      }

  let controller = controller {
    index indexAction
  }
