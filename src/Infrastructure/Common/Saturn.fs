module Saturn
open Microsoft.AspNetCore.Hosting
open Saturn
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

type ApplicationBuilder with
  [<CustomOperation("use_pathbase")>]
  member __.UsePathBase(state: ApplicationState, path: string) =
    if String.IsNullOrEmpty(path) then
      state
    else
      { state with
          AppConfigs = (fun (app : IApplicationBuilder)-> app.UsePathBase(PathString.FromUriComponent(path)))::state.AppConfigs}
  [<CustomOperation("use_app_metrics")>]
  member __.UseAppMetrics(state: ApplicationState, pathBase: string option) =
    { state with
        HostConfigs = (fun (builder: IWebHostBuilder) -> builder.UseMetrics().UseMetricsEndpoints().UseMetricsWebTracking().UseHealth().UseHealthEndpoints())::state.HostConfigs
        ServicesConfig = (fun (services: IServiceCollection) ->
                                  let healtSD = ServiceDescriptor.Singleton<IConfigureOptions<HealthEndpointsHostingOptions>, ConfigureHealthHostingOptions>(fun _ -> ConfigureHealthHostingOptions(pathBase))
                                  let metricsSD = ServiceDescriptor.Singleton<IConfigureOptions<MetricsEndpointsHostingOptions>, ConfigureMetricsHostingOptions>(fun _ -> ConfigureMetricsHostingOptions(pathBase))
                                  services.TryAddEnumerable([| healtSD; metricsSD |])
                                  services ) :: state.ServicesConfig }
