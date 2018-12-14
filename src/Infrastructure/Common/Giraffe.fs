module Giraffe
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open System
open App.Metrics.AspNetCore
open App.Metrics.AspNetCore.Health
open AppMetrics
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection.Extensions
open Microsoft.Extensions.DependencyInjection
open App.Metrics.AspNetCore.Health.Endpoints
open Microsoft.Extensions.Options
open App.Metrics.AspNetCore.Endpoints
open Config
open System

type IWebHostBuilder with
  member __.UseAppMetrics() =
    __.UseMetrics().UseMetricsEndpoints().UseMetricsWebTracking().UseHealth().UseHealthEndpoints()

type IServiceCollection with
  member __.AddAppMetrics(pathBase: string option) =
    let healtSD = ServiceDescriptor.Singleton<IConfigureOptions<HealthEndpointsHostingOptions>, ConfigureHealthHostingOptions>(fun _ -> ConfigureHealthHostingOptions(pathBase))
    let metricsSD = ServiceDescriptor.Singleton<IConfigureOptions<MetricsEndpointsHostingOptions>, ConfigureMetricsHostingOptions>(fun _ -> ConfigureMetricsHostingOptions(pathBase))
    __.TryAddEnumerable([healtSD; metricsSD])
