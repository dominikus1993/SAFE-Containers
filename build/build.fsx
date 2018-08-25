#load ".fake/build.fsx/intellisense.fsx"
#if !FAKE
  #r "netstandard" // Temp fix for https://github.com/fsharp/FAKE/issues/1985
#endif
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open System
open Fake.JavaScript

let dockerUser = Environment.environVar "DockerUser"
let dockerPassword = Environment.environVar "DockerPassword"

let runDockerCompose file operation =
  let result =
      Process.execSimple (fun info ->
          { info with
              FileName = "docker-compose"
              UseShellExecute = false
              Arguments = sprintf "-f %s %s" file operation }
      ) TimeSpan.MaxValue
  if result <> 0 then failwith "Docker build failed"

let runDockerComposeUp file =
  runDockerCompose file "up -d --build"

let runDockerComposeBuild file =
  runDockerCompose file "build"

Target.create "DockerComposeUp:Dev" (fun _ ->
    runDockerComposeUp "../docker-compose.yml"
)

Target.create "DockerComposeBuild:Dev" (fun _ ->
    runDockerComposeBuild "../docker-compose.yml"
)

Target.create "Fable:Start" (fun _ ->
    Npm.run "startClient" (fun param -> { param with WorkingDirectory = "../"})
    //DotNet.exec (fun opt -> opt) "fable webpack-dev-server --port free" "../src/Web/WebSPA" |> ignore
)

Target.create "All" ignore

"DockerComposeUp:Dev"
  ==> "Fable:Start"
  ==> "All"

"DockerComposeBuild:Dev"

Target.runOrDefault "All"
