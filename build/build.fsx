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
open Fake.DotNet
open System

let dockerUser = Environment.environVar "DockerUser"
let dockerPassword = Environment.environVar "DockerPassword"
let solution = "../SAFE-Containers.sln"

let testProjects = [ "../src/Services/Catalog/Catalog.UnitTests/Catalog.UnitTests.fsproj";  "../src/Services/Basket/Basket.UnitTests/Basket.UnitTests.fsproj"]
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

Target.create "Dotnet:InstallSdk" (fun _ ->
  DotNet.install (fun opt -> { opt with Version = DotNet.CliVersion.GlobalJson }) |> ignore
)

Target.create "Dotnet:Restore" (fun _ ->
  DotNet.restore (fun opt -> opt) solution |> ignore
)

Target.create "Dotnet:Build" (fun _ ->
  DotNet.build (fun opt -> opt) solution |> ignore
)

Target.create "Dotnet:RunTests" (fun _ ->
  testProjects |> List.map(fun proj -> async { DotNet.test (fun opt -> opt) proj } ) |> Async.Parallel |> Async.RunSynchronously |> ignore
)

Target.create "All" ignore

"DockerComposeUp:Dev"
  ==> "Fable:Start"
  ==> "All"

"Dotnet:InstallSdk"
  ==> "Dotnet:Restore"
  ==> "Dotnet:Build"
  ==> "Dotnet:RunTests"

"DockerComposeBuild:Dev"

Target.runOrDefault "All"
