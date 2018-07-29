namespace Identity.Api.Domain.Users
open Microsoft.AspNetCore.Identity

type ApplicationUser() =
    inherit IdentityUser()

[<CLIMutable>]
type LoginDto = { username: string; password: string }

[<CLIMutable>]
type RegisterDto = { username:string; email: string; password:string }

type Token = { token: string; expiry: int64 }

module Validation =
    let valdate v =
        let validators = [
            fun u -> if isNull u.username then Some ("username", "Username shound't be empty") else None
            fun u -> if isNull u.password then Some ("password", "Password shound't be empty") else None
        ]
        validators
            |> List.fold(fun acc validator ->
                            match validator v with
                            | Some(k, v) -> Map.add k v acc
                            | None -> acc
        ) Map.empty
