namespace Identity.Api.Auth.Users
open Giraffe.HttpStatusCodeHandlers

module Controller =

    open Microsoft.AspNetCore.Http
    open Giraffe
    open Microsoft.Extensions.Options
    open Identity.Api.Domain.Users
    open Giraffe
    open Identity.Domain.Config
    open Identity.Domain.Users.Helpers
    open FSharp.Control.Tasks.V2

    let handleGetToken =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let! loginDto = ctx.BindModelAsync<LoginDto>()
                let config = ctx.GetService<IOptions<JwtConfig>>()
                let usersRepository = ctx.GetService<IUsersRepository>()
                let! tokenResult = UsersService.loginAsync usersRepository.GetUser (Crypto.jwt(config.Value)) loginDto
                match tokenResult with
                | Ok(token) ->
                     return! Successful.OK token next ctx
                | Error(err) ->
                    return! RequestErrors.BAD_REQUEST err next ctx
            }

    let handleRegister =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let! registerDto = ctx.BindModelAsync<RegisterDto> ()
                let config = ctx.GetService<IOptions<JwtConfig>>()
                let usersRepository = ctx.GetService<IUsersRepository>()
                let! result = UsersService.registerUser usersRepository.RegisterUser (Crypto.jwt(config.Value)) registerDto
                match result with
                | Ok(token) ->
                     return! Successful.created (json token) next ctx
                | Error(err) ->
                    return! RequestErrors.badRequest (text err) next ctx
            }

    let controller: HttpFunc -> HttpContext -> HttpFuncResult =
      choose [
        POST >=> choose [
          route "/" >=> handleRegister
        ]
      ]
