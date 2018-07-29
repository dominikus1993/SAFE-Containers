module Saturn
open Saturn
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open System

type ApplicationBuilder with
  [<CustomOperation("use_pathbase")>]
  member __.UsePathBase(state: ApplicationState, path: string) =
    if String.IsNullOrEmpty(path) then
      state
    else
      { state with
          AppConfigs = (fun (app : IApplicationBuilder)-> app.UsePathBase(PathString.FromUriComponent(path)))::state.AppConfigs}
