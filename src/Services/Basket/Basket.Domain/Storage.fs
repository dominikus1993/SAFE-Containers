namespace Basket.Domain.Storage
open Task

type ICustomerBasketRepository =
  abstract Get : userId:string -> Task<Result<Product, exn>>
