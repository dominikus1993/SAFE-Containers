module Service

open Expecto
open Catalog.Api.Services
open Catalog.Api.Model
open System

[<Tests>]
let tests =
  testList "Services" [
    testList "Product" [
      testCaseAsync "test when OK" <| async {
        let! subject = Product.getBySlug (fun _ -> async { return Ok(Product.Zero())} |> Async.StartAsTask) "" |> Async.AwaitTask
        Expect.isOk subject "should be Ok"
      }
      testCaseAsync "test when Error" <| async {
        let! subject = Product.getBySlug (fun _ -> async { return Error(Exception("Test"))} |> Async.StartAsTask) "" |> Async.AwaitTask
        Expect.isError subject "should be Error"
      }
    ]
  ]
