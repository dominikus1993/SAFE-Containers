module Service
open System.Collections.Generic

[<CLIMutable>]
type Check = { ServiceName: string; Url: string }

[<CLIMutable>]
type ServiceStatus = { Status: string; Healthy: IDictionary<string, string>; }

let download urls = 2
