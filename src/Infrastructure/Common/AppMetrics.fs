module AppMetrics
open System
open Microsoft.Extensions.Options
open App.Metrics.AspNetCore.Health.Endpoints
open App.Metrics.AspNetCore.Endpoints

type ConfigureHealthHostingOptions(pathBase: string option) =
  interface IConfigureOptions<HealthEndpointsHostingOptions> with
    member __.Configure(options) =
      let pingPath, healthPath = match pathBase with
                                 | Some(pathBase) ->
                                   pathBase + "/ping", pathBase + "/health"
                                 | None ->
                                   "/ping", "/health"
      options.HealthEndpoint <- healthPath
      options.PingEndpoint <- pingPath

type ConfigureMetricsHostingOptions(pathBase: string option) =
  interface IConfigureOptions<MetricsEndpointsHostingOptions> with
    member __.Configure(options) =
      let metrics, env, metricstext = match pathBase with
                                      | Some(pathBase) ->
                                        pathBase + "/metrics", pathBase + "/env", pathBase + "/metrics-text"
                                      | None ->
                                        "/metrics", "/env", "/metrics-text"
      options.EnvironmentInfoEndpoint <- env
      options.MetricsEndpoint <- metrics
      options.MetricsTextEndpoint <- metricstext


