module Identity.Api.App

open Microsoft.Extensions.DependencyInjection
open Identity.Api.Domain.Users
open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.Identity.EntityFrameworkCore
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Configuration
open Identity.Domain.Config
open Identity.Api.Auth.Users.Controller
open System
open Giraffe
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.IdentityModel.Tokens
open System.Text
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Hosting

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message


let parsingErrorHandler err = RequestErrors.BAD_REQUEST err

let webApp =
    choose [
        POST >=> choose [
          route "/token" >=> Identity.Api.Auth.Users.Controller.handleGetToken
        ]
        subRoute "/users" Identity.Api.Auth.Users.Controller.controller
        RequestErrors.notFound (text "Not Found") ]

// ---------------------------------
// Main
// ---------------------------------


type Startup(configuration: IConfiguration) =
  member __.ConfigureServices (services : IServiceCollection) =
    services
        .AddResponseCaching()
        .AddGiraffe() |> ignore
    services.AddAuthentication(fun opt ->
                                opt.DefaultAuthenticateScheme <- JwtBearerDefaults.AuthenticationScheme
                                opt.DefaultChallengeScheme <- JwtBearerDefaults.AuthenticationScheme
                              )
            .AddJwtBearer(fun cfg ->
                            let issuer = configuration.["Service:Jwt:Issuer"]
                            let secret = SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.["Service:Jwt:SecretKey"]))
                            cfg.IncludeErrorDetails <- true
                            cfg.TokenValidationParameters <- TokenValidationParameters(ValidateAudience = true, ValidateIssuer = true, ValidIssuer = issuer, ValidAudience = issuer, IssuerSigningKey = secret)
                          ) |> ignore
    services.AddDbContext<IdentityDbContext<ApplicationUser>>(
      fun options ->
        options.UseInMemoryDatabase("Users") |> ignore
    ) |> ignore
    services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<IdentityDbContext<ApplicationUser>>()
        .AddDefaultTokenProviders() |> ignore
    services.Configure<ServiceConfig>(configuration.GetSection("Service")) |> ignore
    services.AddTransient<IUsersRepository>(fun provider ->
                                                    let siginManager = provider.GetService<SignInManager<ApplicationUser>>()
                                                    let userManager = provider.GetService<UserManager<ApplicationUser>>()
                                                    Repository.usersRepository(Identity(userManager, siginManager))
    ) |> ignore
    services.AddAppMetrics( match Environment.getOrElse "PATH_BASE" "" with "" -> None | path -> Some(path))
    services.AddCors() |> ignore

  member __.Configure (app : IApplicationBuilder)
                        (env : IHostingEnvironment)
                        (loggerFactory : ILoggerFactory) =
    app.UseCors(fun policy ->
                  policy.AllowAnyHeader() |> ignore
                  policy.AllowAnyOrigin() |> ignore
                  policy.AllowAnyMethod() |> ignore
                  policy.AllowCredentials() |> ignore
                ) |> ignore
    let pathbase = Environment.getOrElse "PATH_BASE" ""
    if String.IsNullOrEmpty(pathbase) |> not then
      app.UsePathBase(PathString(pathbase)) |> ignore
    app.UseGiraffeErrorHandler(errorHandler)
       .UseStaticFiles()
       .UseResponseCaching()
       .UseGiraffe webApp

let configureLogging (loggerBuilder : ILoggingBuilder) =
    loggerBuilder.AddFilter(fun lvl -> lvl.Equals LogLevel.Error)
                 .AddConsole()
                 .AddDebug() |> ignore

[<EntryPoint>]
let main _ =
    WebHost.CreateDefaultBuilder()
        .UseAppMetrics()
        .UseStartup<Startup>()
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0
