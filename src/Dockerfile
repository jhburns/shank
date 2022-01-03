FROM mcr.microsoft.com/dotnet/sdk:6.0.101-focal AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers to cache
COPY ./src.sln ./
COPY core/core.fsproj ./core/
RUN dotnet restore

COPY .config ./.config
RUN dotnet tool restore

# Copy everything else
COPY . .

# Build project
ENV CI=true
RUN dotnet publish --verbosity quiet ./core/core.fsproj

# Check linting and formating
RUN dotnet tool run dotnet-fsharplint lint ./core/core.fsproj
RUN dotnet tool run fantomas --recurse --check ./

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0.1-focal

WORKDIR /app/core/out
COPY --from=build-env /shank/core/out .

RUN dotnet core.dll