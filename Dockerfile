FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build

COPY ./src /src

WORKDIR /src

RUN dotnet publish . -o /app -c Release

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine

COPY --from=build /app /app

WORKDIR /app

# heroku uses the following
CMD ASPNETCORE_URLS=http://*:$PORT dotnet PigeonAPI.dll