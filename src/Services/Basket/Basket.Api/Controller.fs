module Basket.Api.Controller
open Basket.Domain.Storage
open Basket.Domain.Model.Aggregates
open Basket.Domain.Dto
open System
open System.Security.Claims
open Basket.Domain.Service
open Microsoft.AspNetCore.Http
open Giraffe
open Microsoft.AspNetCore.Mvc
open FSharp.Control.Tasks.V2

module CustomerBasketItem =

  [<CLIMutable>]
  type DeleteItemQuery = { Quantity: int }

  let private addBasketItem (basketId: string) =
    fun (next : HttpFunc) (ctx : HttpContext) ->
      task {
        let userId = (ctx.User.FindFirst ClaimTypes.NameIdentifier).Value |> Guid.Parse
        let! basketItem = ctx.BindModelAsync<CustomerBasketItemDto>()
        let repo =  ctx.GetService<ICustomerBasketRepository>()
        let! basket = CustomerBasket.addItem repo.Get repo.Insert repo.Update (Guid.Parse(basketId), userId) basketItem |> Async.StartAsTask
        match basket with
        | Ok data ->
          return! Successful.OK  (data |> CustomerBasketResponseDto.fromDto) next ctx
        | Error err ->
          return! ServerErrors.internalError (text "Error") next ctx
      }
  let private removeBasketItem (basketId: string) =
    fun (next : HttpFunc)  (ctx: HttpContext) ->
      task {
        let userId = (ctx.User.FindFirst ClaimTypes.NameIdentifier).Value |> Guid.Parse
        let query = ctx.BindQueryString<DeleteItemQuery>()
        let repo =  ctx.GetService<ICustomerBasketRepository>()
        let! basket = CustomerBasket.removeItem repo.Get repo.Update (Guid.Parse(basketId), userId) { ProductId = Guid.Parse(basketId); Quantity = query.Quantity} |> Async.StartAsTask
        match basket with
        | Ok data ->
          return! Successful.OK (data |> CustomerBasketResponseDto.fromDto) next ctx
        | Error err ->
          match err with
          | BasketNotExists ->
            return! Successful.OK (CustomerBasket.zero(Guid.NewGuid())(userId) |> CustomerBasketResponseDto.fromDomain) next ctx
          | _ ->
            return! ServerErrors.internalError (text "Error") next ctx
      }

  let controller =
    choose [
      POST >=>
        choose [
          routef "/%s" addBasketItem
        ]
      DELETE >=>
        choose [
          routef "/%s" removeBasketItem
        ]
    ]
module CustomerBasket =
  let indexAction =
    fun (next : HttpFunc)  (ctx: HttpContext)  ->
      task {
        let userId = (ctx.User.FindFirst ClaimTypes.NameIdentifier).Value |> Guid.Parse
        let repo =  ctx.GetService<ICustomerBasketRepository>()
        match! CustomerBasket.get repo.Get (userId) |> Async.StartAsTask with
        | Ok data ->
          return! Successful.OK ("") next ctx
        | Error err ->
            match err with
            | BasketNotExists ->
              return! Successful.OK (CustomerBasket.zero(Guid.NewGuid())(userId) |> CustomerBasketResponseDto.fromDomain) next ctx
            | _ ->
              return! ServerErrors.internalError (text "Error") next ctx
      }

  let deleteAction(id: string)  =
    fun (next : HttpFunc) (ctx: HttpContext) ->
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
          return! Successful.OK  (data |> CustomerBasketResponseDto.fromDto) next ctx
        | Error err ->
            match err with
            | BasketNotExists ->
              return! Successful.OK (CustomerBasket.zero(Guid.NewGuid())(userId) |> CustomerBasketResponseDto.fromDomain) next ctx
            | _ ->

              return! ServerErrors.internalError (text "Error") next ctx
      }

  let controller: HttpFunc -> HttpContext -> HttpFuncResult =
    choose [
      GET >=>
        choose [
          route  "/" >=> indexAction
        ]
      DELETE >=>
        choose [
          routef "/%s" deleteAction
        ]
      subRoute "/items" CustomerBasketItem.controller ]
