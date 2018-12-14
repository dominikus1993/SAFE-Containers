namespace Identity.Domain.Config

[<CLIMutable>]
type JwtConfig = {
    Issuer: string
    ExpiryDays: float
    Key: string
}

[<CLIMutable>]
type ServiceConfig = { Jwt : JwtConfig }
