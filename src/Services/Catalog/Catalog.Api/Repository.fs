namespace Catalog.Api.Repositories

open System.Threading.Tasks
open Catalog.Api.Model

type IProductRepository =
    abstract member GetBySlug: slug: string -> Task<Product>
