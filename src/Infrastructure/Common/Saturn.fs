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
open Config
open Consul
open System
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
  [<CustomOperation("use_consul")>]
  member __.UseConsul(state: ApplicationState, config: ConsulConfig) =
    let id = Guid.NewGuid().ToString()
    let addClient =  (fun (services: IServiceCollection) ->
                                    services.AddSingleton<IConsulClient>(fun ctx -> new ConsulClient() :> IConsulClient) |> ignore
                                    services
                                  )
    let start = (fun (services: IServiceCollection) ->
                  let pingHealth = AgentServiceCheck( DeregisterCriticalServiceAfter = Nullable<TimeSpan>(TimeSpan.FromMinutes(1.)), Interval = Nullable<TimeSpan>(TimeSpan.FromSeconds(30.)), TCP = config.PingUrl)
                  let health = AgentServiceCheck( DeregisterCriticalServiceAfter = Nullable<TimeSpan>(TimeSpan.FromMinutes(1.)), Interval = Nullable<TimeSpan>(TimeSpan.FromSeconds(30.)), HTTP = config.HealthCheckUrl)
                  let agentReg = AgentServiceRegistration(Address = config.Address, ID = id, Name = config.Name, Port = config.Port, Checks = [| pingHealth; health |])
                  let sp = services.BuildServiceProvider()
                  let client = sp.GetService<IConsulClient>()
                  let appLifetime = sp.GetService<IApplicationLifetime>()
                  appLifetime.ApplicationStarted.Register(fun () -> client.Agent.ServiceRegister(agentReg) |> Async.AwaitTask |> Async.RunSynchronously |> ignore) |> ignore
                  appLifetime.ApplicationStopping.Register(fun () -> client.Agent.ServiceDeregister(agentReg.ID) |> Async.AwaitTask |> Async.RunSynchronously |> ignore) |> ignore
                  services
                )
    { state with ServicesConfig = addClient :: start :: state.ServicesConfig }

