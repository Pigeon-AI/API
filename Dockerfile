FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build

COPY ./src /src

WORKDIR /src

# publish to the currect runtime depending on the architecture
# current supported architectures are x64 and arm64
RUN if [ $(arch) = "aarch64" ] || [ $(arch) = "arm64" ]; \
    then \
        dotnet publish . -o /app -c Release --self-contained -r alpine-arm64; \
    else \
        dotnet publish . -o /app -c Release --self-contained -r alpine-x64; \
    fi


FROM mcr.microsoft.com/dotnet/runtime-deps:6.0-alpine as pigeon-base

COPY --from=build /app /app

WORKDIR /app

# check if it's heroku and needs to do different stuff
ARG HEROKU="false"

# set that build arg as an environment variable so we can read it in the container
ENV HEROKU=${HEROKU}

# if heroku, enable custom port config
CMD if [ "$HEROKU" = true ]; \
    then \
        ASPNETCORE_URLS=http://*:$PORT; \
    fi && \
    ./PigeonAPI
