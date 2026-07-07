FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore as its own layer so it's cached unless a .csproj changes.
COPY global.json ./
COPY src/SilverHub.Api/SilverHub.Api.csproj src/SilverHub.Api/
COPY src/SilverHub.Application/SilverHub.Application.csproj src/SilverHub.Application/
COPY src/SilverHub.Domain/SilverHub.Domain.csproj src/SilverHub.Domain/
COPY src/SilverHub.Infrastructure/SilverHub.Infrastructure.csproj src/SilverHub.Infrastructure/
RUN dotnet restore src/SilverHub.Api/SilverHub.Api.csproj

COPY src/SilverHub.Api/ src/SilverHub.Api/
COPY src/SilverHub.Application/ src/SilverHub.Application/
COPY src/SilverHub.Domain/ src/SilverHub.Domain/
COPY src/SilverHub.Infrastructure/ src/SilverHub.Infrastructure/
RUN dotnet publish src/SilverHub.Api/SilverHub.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "SilverHub.Api.dll"]
