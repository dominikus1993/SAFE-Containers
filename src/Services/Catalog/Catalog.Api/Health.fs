namespace Catalog.Api.Health
open App.Metrics.Health
open FSharp.Control.Tasks.V2
open System.Threading.Tasks
open App.Metrics.Health
open MongoDB.Driver
open System.Linq
open MongoDB.Bson
open System.Threading.Tasks
open MongoDB.Driver
open MongoDB.Driver

type RedisHealthCheck(client: IMongoClient) =
  inherit HealthCheck("MongoDb")

  override this.CheckAsync(ct): HealthCheckResult ValueTask =
    let t = task {
      try
        let db = client.GetDatabase("Catalog")
        let state = client.Cluster.Description.Servers.FirstOrDefault()
        if state |> isNull then
          return HealthCheckResult.Unhealthy("Diconnected")
        else
          let! (result: BsonDocument) = db.RunCommandAsync(Command.op_Implicit("{ping:1}"))
          return HealthCheckResult.Healthy("Conencted")
      with
      | ex -> return HealthCheckResult.Unhealthy(ex)
    }
    ValueTask<HealthCheckResult>(t)
