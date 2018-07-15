namespace Identity.Domain.Config

[<CLIMutable>]
type JwtConfig = {
    issuer: string
    expiryDays: float
    key: string
}