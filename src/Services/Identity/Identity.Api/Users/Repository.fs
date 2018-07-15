namespace Identity.Api.Domain.Users
open Microsoft.AspNetCore.Identity
open FSharp.Control.Tasks
open System.Threading.Tasks

type IUsersRepository =
    abstract member GetUser : username:string -> password:string -> Task<ApplicationUser option>
    abstract member RegisterUser: user:ApplicationUser -> password: string -> Task<Result<unit, string>>

type Storage = 
    | Identity of userManager: UserManager<ApplicationUser> * siginManager: SignInManager<ApplicationUser> 
    
module Repository = 
    open Newtonsoft.Json
    
    let usersRepository = function
        | Identity(usermanager, siginmanager) -> 
            { new IUsersRepository with
                member __.GetUser(username)(password): Task<ApplicationUser option> =
                    task {
                        let! signInResult = siginmanager.PasswordSignInAsync(username, password, false, false)
                        if signInResult.Succeeded then
                            let! user = usermanager.FindByNameAsync(username)
                            return Some(user)
                        else
                            return None
                    }
                member __.RegisterUser(user)(password) =
                    task {
                        let! result = usermanager.CreateAsync(user, password)
                        if result.Succeeded then
                            return Ok()
                         else 
                            return Error(JsonConvert.SerializeObject(result.Errors))
                    }
}