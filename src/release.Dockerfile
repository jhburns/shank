FROM mcr.microsoft.com/dotnet/sdk:6.0.101-focal AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers to cache
COPY ./src.sln ./
COPY core/core.fsproj ./core/
COPY test/test.fsproj ./test/
RUN dotnet restore

COPY .config ./.config
RUN dotnet tool restore

# Copy everything else
COPY . .

RUN dotnet publish -c Release --verbosity quiet

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0.1-focal

WORKDIR /app/
COPY --from=build-env /app/core/out /app/core/out
COPY --from=build-env /app/test/out /app/test/out

RUN ./core/out/core
RUN ./test/out/test