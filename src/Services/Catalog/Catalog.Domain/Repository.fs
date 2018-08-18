namespace Catalog.Api.Repositories

open System.Threading.Tasks
open Catalog.Api.Model
open MongoDB.Driver
open FSharp.Control.Tasks
open MongoDB.Driver.Linq
open System.Linq
open System
open MongoDB.Driver
open MongoDB.Driver

type BrowseProducts =
  { skip : int
    take : int
    sort : string
    priceMin : double option
    priceMax : double option
    name : string option
    tags: string array option }

type IProductRepository =
  abstract GetBySlug : slug:string -> Task<Result<Product, exn>>
  abstract Browse : query:BrowseProducts -> Task<Result<PagedProducts, exn>>

type ITagsRepository =
  abstract All : unit-> Task<Result<Catalog.Api.Model.Tag seq, exn>>

type Storage = MongoDb of database : IMongoDatabase


module Tags =
  [<Literal>]
  let private ProductsCollectionName = "products"

  let storage = function
    | MongoDb db ->
      { new ITagsRepository with
        member __.All() =
          task {
              let col =
                db.GetCollection<Product>(ProductsCollectionName)
              let q = col.AsQueryable()
                        .SelectMany(fun x -> x.Tags)
                        .GroupBy(fun x -> x)
                        .Select(fun x -> { name = x.Key; quantity = x.Count() } )
                        .OrderBy(fun x -> x.name )
              try
                let! p = q.ToListAsync()
                return Ok(p.AsEnumerable())
              with ex -> return Error(ex)
          }}

module Product =
  [<Literal>]
  let private ProductsCollectionName = "products"

  let (|Default|PriceAsc|PriceDesc|NameAsc|NameDesc|) (input : string) =
    match input.ToLower() with
    | "priceasc" -> PriceAsc
    | "pricedesc" -> PriceDesc
    | "nameasc" -> NameAsc
    | "namedesc" -> NameDesc
    | _ -> Default

  let addSortQuery input (q : IMongoQueryable<Product>) =
    match input with
    | Default -> q.OrderBy(fun x -> x.Id)
    | PriceAsc -> q.OrderBy(fun x -> x.Price)
    | PriceDesc -> q.OrderByDescending(fun x -> x.Price)
    | NameAsc -> q.OrderBy(fun x -> x.Name)
    | NameDesc -> q.OrderByDescending(fun x -> x.Name)

  let addNameTextQuery nameOpt (q : IMongoQueryable<Product>) =
    match nameOpt with
    | Some name ->
      let filter = Builders<Product>.Filter.Text(name)
      q.Where(fun _ -> filter.Inject())
    | None -> q

  let addPriceQuery priceMin priceMax (q : IMongoQueryable<Product>) =
    match priceMin, priceMax with
    | Some(min), Some(max) -> q.Where(fun p -> p.Price >= min && p.Price <= max)
    | Some(min), None -> q.Where(fun p -> p.Price >= min)
    | None, Some(max) -> q.Where(fun p -> p.Price <= max)
    | _ -> q

  let addTagsQuery (tagsOpt: string array option)(q : IMongoQueryable<Product>) =
    match tagsOpt with
    | Some(tags) ->
      q.Where(fun p -> p.Tags.Any(fun t -> tags.Contains(t)))
    | None ->
      q

  let storage = function
    | MongoDb db ->
      { new IProductRepository with

          member __.GetBySlug slug =
            task {
              let col = db.GetCollection<Product>(ProductsCollectionName)

              let q =
                query {
                  for product in col.AsQueryable() do
                    where (product.Slug = slug)
                    select product
                }
              try
                let! p = (q :?> IMongoQueryable<Product>).FirstAsync()
                return Ok(p)
              with ex -> return Error(ex)
            }

          member __.Browse browse =
            task {
              let col = db.GetCollection<Product>(ProductsCollectionName)

              let mongoQuery =
                col.AsQueryable()
                |> addNameTextQuery browse.name
                |> addPriceQuery browse.priceMin browse.priceMax
                |> addSortQuery browse.sort
                |> addTagsQuery browse.tags

              let q =
                query {
                  for product in mongoQuery do
                    select product
                    skip browse.skip
                    take browse.take
                }

              try
                let p = (q :?> IMongoQueryable<Product>).ToListAsync()
                let total = mongoQuery.CountAsync()
                do! Task.WhenAll(p, total)
                return Ok({ Products = p.Result.AsEnumerable()
                            TotalItems = total.Result
                            TotalPages = Math.Ceiling((total.Result |> float) / (browse.take |> float)) |> int })
              with ex -> return Error(ex)
            } }
