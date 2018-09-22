module Basket.Api.Config

[<CLIMutable>]
type JwtConfig = {
    issuer: string
    expiryDays: float
    key: string
}
