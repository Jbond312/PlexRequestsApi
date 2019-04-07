FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY src/*.sln .
COPY src/PlexRequests/*.csproj ./PlexRequests/
COPY src/PlexRequests.Core/*.csproj ./PlexRequests.Core/
COPY src/PlexRequests.Store/*.csproj ./PlexRequests.Store/
COPY src/PlexRequests.Api/*.csproj ./PlexRequests.Api/
COPY src/PlexRequests.Plex/*.csproj ./PlexRequests.Plex/
COPY src/PlexRequests.TheMovieDb/*.csproj ./PlexRequests.TheMovieDb/
COPY src/PlexRequests.Settings/*.csproj ./PlexRequests.Settings/
COPY src/PlexRequests.Helpers/*.csproj ./PlexRequests.Helpers/
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


FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime
WORKDIR /app
COPY --from=build /app/PlexRequests/out ./
ENTRYPOINT ["dotnet", "PlexRequests.dll"]
