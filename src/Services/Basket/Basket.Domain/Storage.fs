namespace Basket.Domain.Storage
open FSharp.Control.Tasks.V2
open System.Threading.Tasks
open Basket.Domain.Dto
open StackExchange.Redis
open System
open Microsoft.FSharpLu.Json

type ICustomerBasketRepository =
  abstract Get : customerId:Guid -> Async<Result<CustomerBasketDto, exn>>
  abstract Insert: CustomerBasketDto -> Async<Result<CustomerBasketDto, exn>>
  abstract Update: CustomerBasketDto -> Async<Result<CustomerBasketDto, exn>>
  abstract Remove: CustomerBasketDto -> Async<Result<CustomerBasketDto, exn>>

module CustomerBasket =
  let private getRedisKey customerId = sprintf "{cart/%s}" (customerId.ToString()) |> RedisKey.op_Implicit

  let storage (multiplexer: IConnectionMultiplexer) =
    { new ICustomerBasketRepository with
        member __.Get customerId =
          async {
            let db = multiplexer.GetDatabase()
            let key = getRedisKey customerId
            let! result = db.StringGetAsync(key) |> Async.AwaitTask
            if result.IsNullOrEmpty then
              return Ok(CustomerBasketDto.zero(customerId))
            else
              return Ok(result |> string |> Compact.deserialize)
          }
        member __.Insert basket =
          async {
            let db = multiplexer.GetDatabase()
            let str = basket |> Compact.serialize |> RedisValue.op_Implicit
            let key = basket.CustomerId |> getRedisKey
            let tran = db.CreateTransaction()
            tran.StringSetAsync(key, str) |> ignore
            do! tran.ExecuteAsync() |> Async.AwaitTask |> Async.Ignore
            return Ok(basket)
          }
        member __.Update basket =
          async {
            return! __.Insert(basket)
          }
        member __.Remove basket =
          async {
            let db = multiplexer.GetDatabase()
            let str = basket |> Compact.serialize |> RedisValue.op_Implicit
            let key = basket.CustomerId |> getRedisKey
            let tran = db.CreateTransaction()
            tran.KeyDeleteAsync(key) |> ignore
            do! tran.ExecuteAsync() |> Async.AwaitTask |> Async.Ignore
            return Ok(basket)
          }}
