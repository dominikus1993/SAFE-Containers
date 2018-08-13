namespace Catalog.Api.Repositories

open System.Threading.Tasks
open Catalog.Api.Model
open MongoDB.Driver
open FSharp.Control.Tasks
open MongoDB.Driver.Linq
open System.Linq

type BrowseProducts = { skip: int; take: int; sort: string; priceMin: double option; priceMax: double option }

type IProductRepository =
    abstract member GetBySlug: slug: string -> Task<Result<Product, exn>>
    abstract member Get: query:BrowseProducts -> Task<Result<Product seq, exn>>

type Storage =
    | MongoDb of database: IMongoDatabase

module Product =
  [<Literal>]
  let private ProductsCollectionName = "products"

  let (|Default|PriceAsc|PriceDesc|NameAsc|NameDesc|) (input:string) =
    match input.ToLower() with
    | "priceasc" ->
      PriceAsc
    | "pricedesc" ->
      PriceDesc
    | "nameasc" ->
      NameAsc
    | "namedesc" ->
      NameDesc
    | _ ->
      Default

  let addSortQuery input (q: IMongoQueryable<Product>) =
    match input with
    | Default ->
      q.OrderBy(fun x -> x.Id)
    | PriceAsc ->
      q.OrderBy(fun x -> x.Price)
    | PriceDesc ->
      q.OrderByDescending(fun x -> x.Price)
    | NameAsc ->
      q.OrderBy(fun x -> x.Name)
    | NameDesc ->
      q.OrderByDescending(fun x -> x.Name)

  let addPriceQuery priceMin priceMax (q: IMongoQueryable<Product>)  =
    match priceMin, priceMax with
    | Some(min), Some(max) ->
      q.Where(fun p -> p.Price >= min && p.Price <= max)
    | Some(min), None ->
      q.Where(fun p -> p.Price >= min)
    | None, Some(max) ->
      q.Where(fun p -> p.Price <= max)
    | _ ->
      q

  let storage = function
    | MongoDb db ->
      { new IProductRepository with
          member __.GetBySlug slug =
            task {
              let col = db.GetCollection<Product>(ProductsCollectionName)
              let q = query {
                for product in col.AsQueryable() do
                where (product.Slug = slug)
                select product
              }
              try
                let! p = (q :?> IMongoQueryable<Product>).FirstAsync()
                return Ok(p)
              with
              | ex ->
                return Error(ex)
            }
          member __.Get browse =
            task {
              let col = db.GetCollection<Product>(ProductsCollectionName)
              let mongoQuery = col.AsQueryable() |> addSortQuery browse.sort |> addPriceQuery browse.priceMin browse.priceMax
              let q = query {
                for product in mongoQuery do
                select product
                take browse.take
                skip browse.skip
              }
              try
                let! p = (q :?> IMongoQueryable<Product>).ToListAsync()
                return Ok(p.AsEnumerable())
              with
              | ex ->
                return Error(ex)
            }
     }
