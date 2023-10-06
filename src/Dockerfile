FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /src

COPY *.sln ./

COPY SpotiHub.Api/*.csproj SpotiHub.Api/
COPY SpotiHub.Core.Application/*.csproj SpotiHub.Core.Application/
COPY SpotiHub.Core.Domain/*.csproj SpotiHub.Core.Domain/
COPY SpotiHub.Core.Domain.Contract/*.csproj SpotiHub.Core.Domain.Contract/
COPY SpotiHub.Core.Entity/*.csproj SpotiHub.Core.Entity/
COPY SpotiHub.Infrastructure/*.csproj SpotiHub.Infrastructure/
COPY SpotiHub.Infrastructure.Contract/*.csproj SpotiHub.Infrastructure.Contract/
COPY SpotiHub.Persistence/*.csproj SpotiHub.Persistence/
COPY SpotiHub.Persistence.Migrations/*.csproj SpotiHub.Persistence.Migrations/

RUN dotnet restore -r linux-musl-x64

COPY . .
WORKDIR /src/SpotiHub
RUN dotnet publish -c Release -o /app -r linux-musl-x64 --self-contained false --no-restore ../SpotiHub.sln 

FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine
WORKDIR /app
COPY --from=build /app ./

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS http://*:5100
EXPOSE 5100

ENTRYPOINT ["dotnet", "SpotiHub.Api.dll"]
