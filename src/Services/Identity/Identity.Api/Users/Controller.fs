namespace Identity.Api.Auth.Users

module Controller = 

    open Microsoft.AspNetCore.Http
    open Giraffe
    open Microsoft.Extensions.Options
    open Identity.Api.Domain.Users
    open Saturn.ControllerHelpers
    open Identity.Domain.Config
    open Identity.Domain.Users.Helpers
    open Saturn

    let handleGetToken =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let! loginDto = Controller.getModel<LoginDto> ctx
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
        fun (ctx : HttpContext) ->
            task {
                let! registerDto = Controller.getModel<RegisterDto> ctx
                let config = ctx.GetService<IOptions<JwtConfig>>()
                let usersRepository = ctx.GetService<IUsersRepository>()
                let! result = UsersService.registerUser usersRepository.RegisterUser (Crypto.jwt(config.Value)) registerDto
                match result with
                | Ok(token) -> 
                     return! Response.created ctx token
                | Error(err) ->
                    return! Response.badRequest ctx err
            }
   
    let usersController = controller {
        create (handleRegister)
}