FROM microsoft/dotnet:2.1-aspnetcore-runtime-alpine AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk-stretch AS build
WORKDIR /src
COPY . .
WORKDIR /src/src/Services/Basket/Basket.Api
RUN dotnet restore -nowarn:msb3202,nu1503
RUN dotnet build -c Release -o /app

FROM build as test
WORKDIR /src/src/Services/Basket/Basket.UnitTests
RUN dotnet restore -nowarn:msb3202,nu1503
RUN dotnet test

FROM build AS publish
WORKDIR /src/src/Services/Basket/Basket.Api
RUN dotnet publish --no-restore -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Basket.Api.dll"]
