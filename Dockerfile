FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY src/*.sln .
COPY src/PlexRequests/*.csproj ./PlexRequests/
COPY src/PlexRequests.Core/*.csproj ./PlexRequests.Core/
COPY src/PlexRequests.Store/*.csproj ./PlexRequests.Store/
COPY src/PlexRequests.Api/*.csproj ./PlexRequests.Api/
COPY src/PlexRequests.Plex/*.csproj ./PlexRequests.Plex/
COPY src/PlexRequests.Settings/*.csproj ./PlexRequests.Settings/
COPY src/PlexRequests.Helpers/*.csproj ./PlexRequests.Helpers/
RUN dotnet restore

# copy everything else and build app
COPY ./src ./
WORKDIR /app/
RUN dotnet publish -c Release -o out
RUN cd /app/
RUN ls -a
RUN cd /app/PlexRequests/
RUN ls -a


FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime
WORKDIR /app
COPY --from=build /app/PlexRequests/out ./
RUN ls -a
ENTRYPOINT ["dotnet", "PlexRequests.dll"]
