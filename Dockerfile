# syntax=docker/dockerfile:1

# --- Build stage -----------------------------------------------------------
# Pinned to the SDK feature band declared in global.json for reproducible builds.
FROM mcr.microsoft.com/dotnet/sdk:10.0.300 AS build
WORKDIR /src

# Copy only the project/solution metadata first so `restore` is cached until a
# dependency actually changes.
COPY global.json ./
COPY PolicyPremium.sln ./
COPY src/PolicyPremium.Api/PolicyPremium.Api.csproj src/PolicyPremium.Api/
COPY tests/PolicyPremium.Tests/PolicyPremium.Tests.csproj tests/PolicyPremium.Tests/
RUN dotnet restore PolicyPremium.sln

# Copy the rest of the source and publish the API.
COPY . .
RUN dotnet publish src/PolicyPremium.Api/PolicyPremium.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app

# --- Runtime stage ---------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app ./

# Run as the unprivileged user supplied by the base image.
USER $APP_UID

# Kestrel listens on 8080 by default in the .NET container images.
EXPOSE 8080
ENTRYPOINT ["dotnet", "PolicyPremium.Api.dll"]
