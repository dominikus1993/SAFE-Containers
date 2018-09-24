namespace Basket.Api.Health
open StackExchange.Redis
open App.Metrics.Health
open FSharp.Control.Tasks.V2
open System.Threading.Tasks
open App.Metrics.Health

type RedisHealthCheck(multiplexer: IConnectionMultiplexer) =
  inherit HealthCheck("RedisDb")

  override this.CheckAsync(ct): HealthCheckResult ValueTask =
    let t = task {
      try
        if multiplexer.IsConnected then
          let status = multiplexer.GetStatus()
          if status.Contains("int: ConnectedEstablished") then
            return HealthCheckResult.Healthy("Connected")
          else
            return HealthCheckResult.Unhealthy("Not Connected")
        else
          return HealthCheckResult.Unhealthy("Not Connected")
      with
      | ex -> return HealthCheckResult.Unhealthy(ex)
    }
    ValueTask<HealthCheckResult>(t)
