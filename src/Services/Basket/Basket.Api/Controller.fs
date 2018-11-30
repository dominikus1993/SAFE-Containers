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
open FSharp.Control.Tasks

module CustomerBasketItem =

  [<CLIMutable>]
  type DeleteItemQuery = { Quantity: int }

  let private addBasketItem (basketId: string) =
    fun (ctx: HttpContext) ->
      task {
        let userId = (ctx.User.FindFirst ClaimTypes.NameIdentifier).Value |> Guid.Parse
        let! basketItem = Controller.getModel<CustomerBasketItemDto> ctx
        let repo =  ctx.GetService<ICustomerBasketRepository>()
        let! basket = CustomerBasket.addItem repo.Get repo.Insert repo.Update (Guid.Parse(basketId), userId) basketItem |> Async.StartAsTask
        match basket with
        | Ok data ->
          return Response.ok ctx (data |> CustomerBasketResponseDto.fromDto)
        | Error err ->
          return Response.internalError ctx "Error"
      }
  let private removeBasketItem (basketId: string) =
    fun (ctx: HttpContext) (id: string) ->
      task {
        let userId = (ctx.User.FindFirst ClaimTypes.NameIdentifier).Value |> Guid.Parse
        let query = Controller.getQuery<DeleteItemQuery> ctx
        let repo =  ctx.GetService<ICustomerBasketRepository>()
        let! basket = CustomerBasket.removeItem repo.Get repo.Update (Guid.Parse(basketId), userId) { ProductId = Guid.Parse(id); Quantity = query.Quantity} |> Async.StartAsTask
        match basket with
        | Ok data ->
          return Response.ok ctx (data |> CustomerBasketResponseDto.fromDto)
        | Error err ->
          match err with
          | BasketNotExists ->
            return Response.ok ctx (CustomerBasket.zero(Guid.NewGuid())(userId) |> CustomerBasketResponseDto.fromDomain)
          | _ ->
            return Response.internalError ctx "Error"
      }
  let controller (basketId: string) =
    controller {
       create (addBasketItem basketId)
       delete (removeBasketItem basketId)
    }

module CustomerBasket =
  let private indexAction =
    fun (ctx : HttpContext) ->
      task {
        let userId = (ctx.User.FindFirst ClaimTypes.NameIdentifier).Value |> Guid.Parse
        let repo =  ctx.GetService<ICustomerBasketRepository>()
        match! CustomerBasket.get repo.Get (userId) |> Async.StartAsTask with
        | Ok data ->
          return! Response.ok ctx ("")
        | Error err ->
            match err with
            | BasketNotExists ->
              return! Response.ok ctx (CustomerBasket.zero(Guid.NewGuid())(userId) |> CustomerBasketResponseDto.fromDomain)
            | _ ->
              return! Response.internalError ctx "Error"
      }

  let private deleteAction =
    fun (ctx : HttpContext) (id: string) ->
      task {
        let userId = (ctx.User.FindFirst ClaimTypes.NameIdentifier).Value |> Guid.Parse
        let repo =  ctx.GetService<ICustomerBasketRepository>()
        let! r = CustomerBasket.get repo.Get (userId)
                  |> AsyncResult.bind(fun basket ->
                                        async {
                                          return! CustomerBasket.remove repo.Remove (basket)
                                        })
                  |> Async.StartAsTask
        match r with
        | Ok data ->
          return! Response.ok ctx (data |> CustomerBasketResponseDto.fromDto)
        | Error err ->
            match err with
            | BasketNotExists ->
              return! Response.ok ctx (CustomerBasket.zero(Guid.NewGuid())(userId) |> CustomerBasketResponseDto.fromDomain)
            | _ ->

              return! Response.internalError ctx "Error"
      }

  let controller = controller {
    subController "/items" CustomerBasketItem.controller
    index indexAction
    delete deleteAction
  }
