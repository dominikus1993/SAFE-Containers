namespace Catalog.Api.Repositories

open System.Threading.Tasks
open Catalog.Api.Model
open MongoDB.Driver
open FSharp.Control.Tasks
open MongoDB.Driver.Linq
open System.Linq

type BrowseProducts = { skip: int; take: int }

type IProductRepository =
    abstract member GetBySlug: slug: string -> Task<Result<Product, exn>>
    abstract member Get: query:BrowseProducts -> Task<Result<Product seq, exn>>

type Storage =
    | MongoDb of database: IMongoDatabase

module Product =
  [<Literal>]
  let private ProductsCollectionName = "products"

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
              let q = query {
                for product in col.AsQueryable() do
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
