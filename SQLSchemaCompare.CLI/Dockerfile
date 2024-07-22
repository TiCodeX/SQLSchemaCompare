FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS base
WORKDIR /app

# Install Timezone data
RUN apk add --update tzdata

# Workaround Unhandled exception. System.NotSupportedException: Globalization Invariant Mode is not supported.
# https://github.com/dotnet/SqlClient/issues/220
RUN apk add --update icu-libs
RUN apk add icu-data-full
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

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
RUN dotnet restore -r linux-musl-x64 SQLSchemaCompare.Core/SQLSchemaCompare.Core.csproj
RUN dotnet restore -r linux-musl-x64 SQLSchemaCompare.Services/SQLSchemaCompare.Services.csproj
RUN dotnet restore -r linux-musl-x64 SQLSchemaCompare.Infrastructure/SQLSchemaCompare.Infrastructure.csproj
RUN dotnet restore -r linux-musl-x64 SQLSchemaCompare.CLI/SQLSchemaCompare.CLI.csproj

# Copy the rest of the code (we only copy the necessary files to make good use of layer caching)
COPY SQLSchemaCompare.Core/ SQLSchemaCompare.Core
COPY SQLSchemaCompare.Services/ SQLSchemaCompare.Services
COPY SQLSchemaCompare.Infrastructure/ SQLSchemaCompare.Infrastructure
COPY SQLSchemaCompare.CLI/ SQLSchemaCompare.CLI

WORKDIR /src/SQLSchemaCompare.CLI

RUN dotnet build --no-restore SQLSchemaCompare.CLI.csproj -c Release

FROM build AS publish
RUN dotnet publish --no-restore --no-build SQLSchemaCompare.CLI.csproj -c Release -o /app/publish

FROM base AS final
COPY --from=publish /app/publish .

ENTRYPOINT ["/app/TiCodeX.SQLSchemaCompare.CLI"]
