FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY src/*.sln .
COPY src/PlexRequests/*.csproj ./PlexRequests/
COPY src/PlexRequests.Core/*.csproj ./PlexRequests.Core/
COPY src/PlexRequests.Repository/*.csproj ./PlexRequests.Repository/
COPY src/PlexRequests.Api/*.csproj ./PlexRequests.Api/
COPY src/PlexRequests.Plex/*.csproj ./PlexRequests.Plex/
COPY src/PlexRequests.TheMovieDb/*.csproj ./PlexRequests.TheMovieDb/
COPY src/PlexRequests.Sync/*.csproj ./PlexRequests.Sync/

COPY src/PlexRequests.UnitTests/*.csproj ./PlexRequests.UnitTests/
COPY src/PlexRequests.Sync.UnitTests/*.csproj ./PlexRequests.Sync.UnitTests/
COPY src/PlexRequests.Core.UnitTests/*.csproj ./PlexRequests.Core.UnitTests/
RUN dotnet restore

# copy everything else and build app
COPY ./src ./
WORKDIR /app/
RUN dotnet publish -c Release -o out

# run tests
RUN dotnet test

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "PlexRequests.dll"]
