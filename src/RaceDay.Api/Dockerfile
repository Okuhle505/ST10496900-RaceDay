FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish RaceDay.Api.csproj -c Release -o /out
FROM base
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "RaceDay.Api.dll"]
