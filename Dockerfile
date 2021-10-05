FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine-amd64 AS build

COPY ./src /src

WORKDIR /src

RUN dotnet publish . -o /app -c Release -r alpine-x64 --self-contained

FROM mcr.microsoft.com/dotnet/runtime-deps:6.0-alpine-amd64

COPY --from=build /app /app

WORKDIR /app

CMD ./PigeonAPI