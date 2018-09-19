namespace Basket.Domain.Storage
open FSharp.Control.Tasks.V2
open System.Threading.Tasks
open Basket.Domain.Dto
open StackExchange.Redis
open System
open Microsoft.FSharpLu.Json

type ICustomerBasketRepository =
  abstract Get : customerId:Guid -> Task<Result<CustomerBasketDto, exn>>
  abstract Insert: CustomerBasketDto -> Task<Result<CustomerBasketDto, exn>>
  abstract Update: CustomerBasketDto -> Task<Result<CustomerBasketDto, exn>>
  abstract Remove: CustomerBasketDto -> Task<Result<CustomerBasketDto, exn>>

module CustomerBasket =
  let private getRedisKey customerId = sprintf "{cart/%s}" (customerId.ToString()) |> RedisKey.op_Implicit

  let storage (multiplexer: IConnectionMultiplexer) =
    { new ICustomerBasketRepository with
        member __.Get customerId =
          task {
            let db = multiplexer.GetDatabase()
            let key = getRedisKey customerId
            let! result = db.StringGetAsync(key)
            if result.IsNullOrEmpty then
              return Ok(CustomerBasketDto.zero(customerId))
            else
              return Ok(result |> string |> Compact.deserialize)
          }
        member __.Insert basket =
          task {
            let db = multiplexer.GetDatabase()
            let str = basket |> Compact.serialize |> RedisValue.op_Implicit
            let key = basket.CustomerId |> getRedisKey
            let tran = db.CreateTransaction()
            tran.StringSetAsync(key, str) |> ignore
            do! tran.ExecuteAsync() :> Task
            return Ok(basket)
          }
        member __.Update basket =
          task {
            return! __.Insert(basket)
          }
        member __.Remove basket =
          task {
            let db = multiplexer.GetDatabase()
            let str = basket |> Compact.serialize |> RedisValue.op_Implicit
            let key = basket.CustomerId |> getRedisKey
            let tran = db.CreateTransaction()
            tran.KeyDeleteAsync(key) |> ignore
            do! tran.ExecuteAsync() :> Task
            return Ok(basket)
          }}
