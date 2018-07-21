namespace Catalog.Api.Controllers
open Saturn
open FSharp.Control.Tasks

type Response = {
    a: string
    b: string
}

module Products =
  let showAction slug = 2

  let controller = controller {
    index (fun _ -> task {
        return { a = "hello"; b = "world"}
    })
  }
