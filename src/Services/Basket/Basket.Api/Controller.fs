module Basket.Api.Controller
open Basket.Domain.Storage
open Basket.Domain.Model.Aggregates
open Basket.Domain.Dto
open System
open System.Security.Claims
open Basket.Domain.Service
open Microsoft.AspNetCore.Http
open Saturn
open Giraffe
open Microsoft.AspNetCore.Mvc

module CustomerBasketItem =
  let private addBasketItem =
    fun (ctx: HttpContext) ->
      task {
        let userId = (ctx.User.FindFirst ClaimTypes.NameIdentifier).Value |> Guid.Parse
        let! basketItem = Controller.getModel<CustomerBasketItemDto> ctx
        let repo =  ctx.GetService<ICustomerBasketRepository>()
        let! basket = CustomerBasket.addItem repo.Get repo.Insert repo.Update (Guid.NewGuid(), userId) basketItem |> Async.StartAsTask
        match basket with
        | Ok data ->
          return Response.created ctx data
        | Error err ->
          return Response.internalError ctx "Error"
      }

  let controller (basketId: string) =
    controller {
       create addBasketItem
    }

module CustomerBasket =
  let private indexAction =
    fun (ctx : HttpContext) ->
      task {
        let userId = (ctx.User.FindFirst ClaimTypes.NameIdentifier).Value |> Guid.Parse
        let repo =  ctx.GetService<ICustomerBasketRepository>()
        match! CustomerBasket.get repo.Get (userId) |> Async.StartAsTask with
        | Ok data ->
          return! Response.ok ctx (data)
        | Error err ->
            match err with
            | BasketNotExists ->
              return! Response.ok ctx (CustomerBasketDto.zero(Guid.NewGuid())(userId))
            | _ ->
              return! Response.internalError ctx "Error"
      }

  let controller = controller {
    subController "/items" CustomerBasketItem.controller
    index indexAction
  }
