FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY src/*.sln .
COPY src/PlexRequestsApi/*.csproj ./PlexRequestsApi/
COPY src/PlexRequestsApi.Core/*.csproj ./PlexRequestsApi.Core/
COPY src/PlexRequestsApi.Store/*.csproj ./PlexRequestsApi.Store/
COPY src/PlexRequestsApi.Plex/*.csproj ./PlexRequestsApi.Plex/
RUN dotnet restore

# copy everything else and build app
COPY ./src ./
WORKDIR /app/
RUN dotnet publish -c Release -o out
RUN cd /app/
RUN ls -a
RUN cd /app/PlexRequestsApi/
RUN ls -a


FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime
WORKDIR /app
COPY --from=build /app/PlexRequestsApi/out ./
RUN ls -a
ENTRYPOINT ["dotnet", "PlexRequestsApi.dll"]
