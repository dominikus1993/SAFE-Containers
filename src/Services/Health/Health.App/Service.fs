module Service
open System.Collections.Generic
open System.Collections
open Hopac
open HttpFs.Client
open System
open Microsoft.FSharpLu.Json

[<CLIMutable>]
type Check = { ServiceName: string; Url: string }

[<CLIMutable>]
type ServiceStatus = { Status: string; Healthy: IDictionary<string, string>; UnHealthy: IDictionary<string, string> }

type CheckResult = { ServiceName: string; Status: ServiceStatus }

let downloadOne check =
  job {
    let uri = Uri(check.Url)
    let! req = Request.create Get uri |> Request.responseAsString
    return { ServiceName = check.ServiceName; Status = req |> Compact.deserialize }
  }

let download checks =
  checks
    |> List.map(downloadOne)
    |> Job.conCollect
    |> Hopac.startAsTask
