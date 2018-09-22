namespace Basket.Domain.Storage
open FSharp.Control.Tasks.V2
open System.Threading.Tasks
open Basket.Domain.Dto
open StackExchange.Redis
open System
open Microsoft.FSharpLu.Json
open Basket.Domain.Messages

type ICustomerBasketRepository =
  abstract Get : customerId:Guid -> Async<Result<CustomerBasketDto, ErrorMessage>>
  abstract Insert: CustomerBasketDto -> Async<Result<CustomerBasketDto, ErrorMessage>>
  abstract Update: CustomerBasketDto -> Async<Result<CustomerBasketDto, ErrorMessage>>
  abstract Remove: CustomerBasketDto -> Async<Result<CustomerBasketDto, ErrorMessage>>

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
              return Error(BasketNotExists)
            else
              return Ok(result |> string |> Compact.deserialize)
          }
        member __.Insert basket =
          async {
            let db = multiplexer.GetDatabase()
            let str = basket |> Compact.serialize |> RedisValue.op_Implicit
            let key = basket.CustomerId |> getRedisKey
            let tran = db.CreateTransaction()
            tran.StringSetAsync(key, str, Nullable(TimeSpan.MaxValue), When.NotExists) |> ignore
            do! tran.ExecuteAsync() |> Async.AwaitTask |> Async.Ignore
            return Ok(basket)
          }
        member __.Update basket =
          async {
            let db = multiplexer.GetDatabase()
            let str = basket |> Compact.serialize |> RedisValue.op_Implicit
            let key = basket.CustomerId |> getRedisKey
            let tran = db.CreateTransaction()
            tran.StringSetAsync(key, str, Nullable(TimeSpan.MaxValue), When.Exists) |> ignore
            do! tran.ExecuteAsync() |> Async.AwaitTask |> Async.Ignore
            return Ok(basket)
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
