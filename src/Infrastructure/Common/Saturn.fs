module Saturn
open Microsoft.AspNetCore.Hosting
open Saturn
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open System
open App.Metrics.AspNetCore
open App.Metrics.AspNetCore.Health
type ApplicationBuilder with
  [<CustomOperation("use_pathbase")>]
  member __.UsePathBase(state: ApplicationState, path: string) =
    if String.IsNullOrEmpty(path) then
      state
    else
      { state with
          AppConfigs = (fun (app : IApplicationBuilder)-> app.UsePathBase(PathString.FromUriComponent(path)))::state.AppConfigs}
  [<CustomOperation("use_app_metrics")>]
  member __.UseAppMetrics(state: ApplicationState) =
    { state with HostConfigs = (fun (builder: IWebHostBuilder) -> builder.UseMetrics().UseMetricsEndpoints().UseMetricsWebTracking().UseHealth().UseHealthEndpoints())::state.HostConfigs }
