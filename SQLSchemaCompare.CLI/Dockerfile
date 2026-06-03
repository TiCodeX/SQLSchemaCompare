FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine AS base
WORKDIR /app

# Install dependencies
RUN apk add --no-cache \
    tzdata \
    icu-libs \
    icu-data-full \
    krb5-libs

# Workaround Unhandled exception. System.NotSupportedException: Globalization Invariant Mode is not supported.
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /src

# Copy solution-wide files
COPY .editorconfig .editorconfig
COPY global.json global.json
COPY Directory.Build.props Directory.Build.props
COPY SonarLint.xml SonarLint.xml

# We only copy the csproj files first so the layer caching will work correctly
COPY ["SQLSchemaCompare.Core/SQLSchemaCompare.Core.csproj", "SQLSchemaCompare.Core/"]
COPY ["SQLSchemaCompare.Services/SQLSchemaCompare.Services.csproj", "SQLSchemaCompare.Services/"]
COPY ["SQLSchemaCompare.Infrastructure/SQLSchemaCompare.Infrastructure.csproj", "SQLSchemaCompare.Infrastructure/"]
COPY ["SQLSchemaCompare.CLI/SQLSchemaCompare.CLI.csproj", "SQLSchemaCompare.CLI/"]

# restore the projects (this will be very fast with layer caching if the csproj files haven't changed)
RUN dotnet restore --locked-mode SQLSchemaCompare.CLI

# Copy the rest of the code (we only copy the necessary files to make good use of layer caching)
COPY SQLSchemaCompare.Core/ SQLSchemaCompare.Core
COPY SQLSchemaCompare.Services/ SQLSchemaCompare.Services
COPY SQLSchemaCompare.Infrastructure/ SQLSchemaCompare.Infrastructure
COPY SQLSchemaCompare.CLI/ SQLSchemaCompare.CLI

RUN dotnet build --no-restore SQLSchemaCompare.CLI -r linux-musl-x64 -c Release

FROM build AS publish
RUN dotnet publish --no-restore --no-build SQLSchemaCompare.CLI -r linux-musl-x64 -c Release -o /app/publish

FROM base AS final
COPY --from=publish /app/publish .

ENTRYPOINT ["/app/TiCodeX.SQLSchemaCompare.CLI"]
