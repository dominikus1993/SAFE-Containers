namespace Identity.Api.Domain.Users
open FSharp.Control.Tasks
open System.Threading.Tasks

module UsersService = 
    let loginAsync (getUser: string -> string -> Task<ApplicationUser option>) (generateToken: string -> Token) (loginDto: LoginDto) : Task<Result<Token, string>> =
        task {
            let! userOption = getUser loginDto.username loginDto.password
            match userOption with 
            | Some user ->
                let token = generateToken(user.UserName)
                return Ok(token)
            | None -> 
                return Error("username or password are incorrect")
        }
    
    let registerUser(storeUser: ApplicationUser -> string -> Task<Result<unit, string>>)(generateToken: string -> Token) (register: RegisterDto) =
        task {
            let user = ApplicationUser(Email = register.email, UserName = register.username)
            let! registrationResult = storeUser user register.password
            return registrationResult |> Result.map(fun () -> generateToken(register.username))
}