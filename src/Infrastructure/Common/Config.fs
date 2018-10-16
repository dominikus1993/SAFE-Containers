module Config

type ConsulConfig = { Address: string; Name: string; Port: int; HealthCheckUrl: string; PingUrl: string; Enabled: bool}
