FROM microsoft/dotnet:2.1-aspnetcore-runtime-alpine AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk-stretch AS build
WORKDIR /src
COPY . .
WORKDIR /src/src/Services/Catalog/Catalog.Import
RUN dotnet restore -nowarn:msb3202,nu1503
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish --no-restore -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Catalog.Import.dll", "-q 10000"]
