namespace Catalog.Api.Repositories

open System.Threading.Tasks
open Catalog.Api.Model
open MongoDB.Driver
open FSharp.Control.Tasks
open MongoDB.Driver.Linq

type IProductRepository =
    abstract member GetBySlug: slug: string -> Task<Result<Product, exn>>

type Storage =
    | MongoDb of database: IMongoDatabase

module Product =

  let storage = function
    | MongoDb db ->
      { new IProductRepository with
          member __.GetBySlug slug =
            task {
              let col = db.GetCollection<Product>("products")
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
            }
