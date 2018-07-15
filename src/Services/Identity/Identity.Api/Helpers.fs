namespace Identity.Domain.Users.Helpers
open System.Security.Cryptography
open System
open System.Security.Claims
open System.IdentityModel.Tokens.Jwt
open Microsoft.IdentityModel.Tokens
open System.Text
open Identity.Api.Domain.Users
open Identity.Domain.Config

module Crypto =

    [<Literal>]
    let internal SaltSize = 40

    [<Literal>]
    let internal DeriveBytesIterationsCount = 10000; 

    let toEpoch(date: DateTime) =  
        date.Subtract(DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds |> int64

    let salt() =
        let saltBytes: byte array = Array.zeroCreate SaltSize
        let rng = RandomNumberGenerator.Create()
        rng.GetBytes(saltBytes)
        Convert.ToBase64String(saltBytes)
    
    let hashPassword (password: string, salt: string)  =       
        if salt |> String.IsNullOrEmpty || password |> String.IsNullOrEmpty then
            failwith "salt or password is empty"
        else
            let bytes: byte array = Array.zeroCreate (password.Length * sizeof<char>)
            Buffer.BlockCopy(password.ToCharArray(), 0, bytes, 0, bytes.Length);
            use pbkdf2 = new Rfc2898DeriveBytes(password, bytes, DeriveBytesIterationsCount);
            Convert.ToBase64String(pbkdf2.GetBytes(SaltSize))
            
    let jwt (config: JwtConfig) (username: string): Token = 
        let now = DateTime.UtcNow
        let expiry = now.AddDays(config.expiryDays)

        let claims = [| Claim(JwtRegisteredClaimNames.Sub, username); Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) |]
        let key = SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.key))
        let credentials = SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        let token = JwtSecurityToken(issuer = config.issuer,  audience = config.issuer, claims = claims, expires = Nullable<DateTime>(expiry), signingCredentials = credentials)

        let jwt = JwtSecurityTokenHandler().WriteToken(token)
        { token = jwt; expiry = expiry |> toEpoch }