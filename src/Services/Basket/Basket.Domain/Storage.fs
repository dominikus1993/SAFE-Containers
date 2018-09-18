namespace Basket.Domain.Storage
open FSharp.Control.Tasks.V2
open System.Threading.Tasks
open Basket.Domain.Dto
open System.Threading.Tasks
open StackExchange.Redis
open Basket.Domain.Dto
open System
open Microsoft.FSharpLu.Json

type ICustomerBasketRepository =
  abstract Get : userId:Guid -> Task<Result<CustomerBasketDto, exn>>
  abstract Insert: CustomerBasketDto -> Task<Result<CustomerBasketDto, exn>>
  abstract Update: CustomerBasketDto -> Task<Result<CustomerBasketDto, exn>>
  abstract Remove: CustomerBasketDto -> Task<Result<CustomerBasketDto, exn>>

module CustomerBasket =
  let storage (multiplexer: IConnectionMultiplexer) =
    { new ICustomerBasketRepository with
        member __.Get userId =
          task {
            let db = multiplexer.GetDatabase()
            let key = sprintf "{cart/%s}" (userId.ToString()) |> RedisKey.op_Implicit
            let! result = db.StringGetAsync(key)
            if result.IsNullOrEmpty then
              return Ok(CustomerBasketDto.zero(userId))
            else
              return Ok(result |> string |> Compact.deserialize)
          }
        member __.Insert basket =
          task {
            return Error(Exception("daasd"))
          }
        member __.Update basket =
          task {
            return Error(Exception("daasd"))
          }
        member __.Remove basket =
          task {
            return Error(Exception("daasd"))
          }}
