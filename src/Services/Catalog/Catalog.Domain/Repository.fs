namespace Catalog.Api.Repositories

open System.Threading.Tasks
open Catalog.Api.Model
open MongoDB.Driver

type IProductRepository =
    abstract member GetBySlug: slug: string -> Task<Product option>

type Storage =
    | MongoDb of database: IMongoDatabase

module Product =

  let storage = function
    | MongoDb db ->
      { new IProductRepository with
          member __.GetBySlug slug =
            Task.FromResult(None) }
