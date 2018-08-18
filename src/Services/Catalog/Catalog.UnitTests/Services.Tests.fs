module Service

open Expecto
open Catalog.Api.Services
open Catalog.Api.Model
open System
open Catalog.Api.Repositories
open System.Threading.Tasks

[<Tests>]
let tests =
  testList "Services" [
    testList "Tags" [
      testList "all" [
        testCaseAsync "test when OK" <| async {
          let f() = async { return Ok( seq { yield { name = "Test"; quantity = 1 } }) } |> Async.StartAsTask
          let! subject = Tags.all f |> Async.AwaitTask
          Expect.isOk subject "should be Ok"
        }
        testCaseAsync "test when Error" <| async {
          let f() = async { return Error(Exception("Test"))} |> Async.StartAsTask
          let! subject = Tags.all f |> Async.AwaitTask
          Expect.isError subject "should be Error"
        }
      ]
    ]
    testList "Product" [
      testList "getBySlug" [
        testCaseAsync "test when OK" <| async {
          let! subject = Product.getBySlug (fun _ -> async { return Ok(Product.Zero())} |> Async.StartAsTask) "" |> Async.AwaitTask
          Expect.isOk subject "should be Ok"
        }
        testCaseAsync "test when Error" <| async {
          let! subject = Product.getBySlug (fun _ -> async { return Error(Exception("Test"))} |> Async.StartAsTask) "" |> Async.AwaitTask
          Expect.isError subject "should be Error"
        }
      ]
      testList "browse" [
        testCaseAsync "test when OK and parameters has corect values" <| async {
          let f( b: BrowseProducts): Task<Result<PagedProducts, exn>> =
            async {
              Expect.equal b.name (Some("Test")) "Name should be some"
              Expect.equal b.priceMax (Some(1.)) "PriceMax should be some"
              Expect.equal b.priceMin (Some(1.)) "PriceMin should be some"
              Expect.equal b.skip (0) "Skip should be 0"
              Expect.equal b.sort ("default") "Sort should be default"
              Expect.equal b.tags (Some([| "test" |])) "Tags should be some"
              Expect.equal b.take (15) "take should be 15"
              return Ok(  { Products = seq { yield Product.Zero() } ; TotalItems = 1; TotalPages = 1 })
            } |> Async.StartAsTask
          let! subject = Product.get f ({ pageSize = Some(15); pageIndex = Some(1); sort = Some("default"); priceMin = Some(1.); priceMax = Some(1.); name = Some("Test"); tags = Some("test") }) |> Async.AwaitTask
          Expect.isOk subject "result should be ok"
        }
        testCaseAsync "test when OK and parameters has default values" <| async {
          let f( b: BrowseProducts): Task<Result<PagedProducts, exn>> =
            async {
              Expect.equal b.name (None) "Name should be some"
              Expect.equal b.priceMax (None) "PriceMax should be some"
              Expect.equal b.priceMin (None) "PrcieMin should be some"
              Expect.equal b.skip (0) "Skip should be 0"
              Expect.equal b.sort ("default") "Sort should be default"
              Expect.equal b.tags (None) "tags should be none"
              Expect.equal b.take (15) "Take should be 15"
              return Ok(  { Products = seq { yield Product.Zero() } ; TotalItems = 1; TotalPages = 1 })
            } |> Async.StartAsTask
          let! subject = Product.get f ({ pageSize = None; pageIndex = None; sort = None; priceMin = None; priceMax = None; name = None; tags = None }) |> Async.AwaitTask
          Expect.isOk subject "result should be ok"
        }
        testCaseAsync "test when Error" <| async {
          let f( b: BrowseProducts): Task<Result<PagedProducts, exn>> =
            async {
              Expect.equal b.name (None) "Name should be some"
              Expect.equal b.priceMax (None) "PriceMax should be some"
              Expect.equal b.priceMin (None) "PrcieMin should be some"
              Expect.equal b.skip (0) "Skip should be 0"
              Expect.equal b.sort ("default") "Sort should be default"
              Expect.equal b.tags (None) "tags should be none"
              Expect.equal b.take (15) "Take should be 15"
              return Error(Exception("Test"))
            } |> Async.StartAsTask
          let! subject = Product.get f ({ pageSize = None; pageIndex = None; sort = None; priceMin = None; priceMax = None; name = None; tags = None }) |> Async.AwaitTask
          Expect.isError subject "result should be error"
        }
      ]
    ]
  ]
