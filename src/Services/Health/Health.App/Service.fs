module Service
open System.Collections.Generic
open System.Collections

[<CLIMutable>]
type Check = { ServiceName: string; Url: string }

[<CLIMutable>]
type ServiceStatus = { Status: string; Healthy: IDictionary<string, string>; UnHealthy: IDictionary<string, string> }

type CheckResult = { Overall: string; }

let download urls = 2
