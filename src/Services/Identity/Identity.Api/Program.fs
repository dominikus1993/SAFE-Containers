module Identity.Api.App

open System
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Identity.Api.Domain.Users
open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.Identity.EntityFrameworkCore
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Configuration
open Saturn
open Identity.Domain.Config
open Identity.Api.Auth.Users.Controller

// ---------------------------------
// Configuration
// ---------------------------------
let configuration =
    ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build()

let configureServices (services : IServiceCollection) =
    services.AddDbContext<IdentityDbContext<ApplicationUser>>(
            fun options ->
                options.UseInMemoryDatabase("Users") |> ignore
            ) |> ignore
    services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<IdentityDbContext<ApplicationUser>>()
        .AddDefaultTokenProviders() |> ignore
    services.Configure<JwtConfig>(configuration.GetSection("jwt")) |> ignore
    services.AddTransient<IUsersRepository>(fun provider ->
                                                    let siginManager = provider.GetService<SignInManager<ApplicationUser>>()
                                                    let userManager = provider.GetService<UserManager<ApplicationUser>>()
                                                    Repository.usersRepository(Identity(userManager, siginManager))
                                            ) |> ignore
    services



let topRouter = router {
    post "/token" handleGetToken
    forward "/users" usersController
}

let app = application {
    use_router topRouter
    use_pathbase (Environment.getOrElse "PATH_BASE" "")
    url (Environment.getOrElse "API_URL" "http://0.0.0.0:8085/")
    service_config (configureServices)
}

[<EntryPoint>]
let main _ =
    run app
    0
