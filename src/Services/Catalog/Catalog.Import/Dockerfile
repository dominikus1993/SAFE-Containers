FROM microsoft/dotnet:2.2-aspnetcore-runtime-alpine AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk-stretch AS build
WORKDIR /src
COPY . .
RUN dotnet tool install --tool-path ".paket" paket
ENV PATH="/root/.dotnet/tools:${PATH}"

WORKDIR /src/src/Services/Catalog/Catalog.Import
RUN dotnet restore -nowarn:msb3202,nu1503
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish --no-restore -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Catalog.Import.dll"]
