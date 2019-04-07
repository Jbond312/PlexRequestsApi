# PlexRequestsApi (name TBC)

## Note - Work in progress

This project contains a Web Api to allow for Plex server administrators to provide functionality to their users so they can request new content or flag existing content for review. Users should also be notified when requests are fulfilled and be able to see if content being requested is already available within Plex.

## Data Providers

PlexRequests needs to be able to gather both TV and Movie data from varying 3rd party API's. The main purpose is to allow for users to request new content and the other is to try and match up existing Plex content to the data from the 3rd party API's to identify content already available on the server.

The following popular API's exist for retrieving metadata:

- [TheMovieDb](https://www.themoviedb.org/)
- [TheTvDb](https://www.thetvdb.com/)
- [Trakt.Tv](https://trakt.tv/dashboard)
- [TVMaze](https://www.tvmaze.com/)
- [Fanart.tv (Images)](https://fanart.tv/)

To begin with TheMovieDb will be the primary data provider for all content.

## Getting Started

If you want to run the project in its existing state, it can be built and deployed using docker.

### Pre-requisites

- Docker
- .NET Core 2.2
- MongoDb instance that is accessible from the plexrequests docker container

### Configuring settings

All of the settings for the application can currently be found within the [PlexRequests\appsettings.json](https://github.com/Jbond312/PlexRequestsApi/blob/develop/src/PlexRequests/appsettings.json) file and should be updated before running the Api. When running in `Production` environment variables should be specified instead for the MongoDb username and password (see example below). This configuration is temporary in the projects current state.

### Build & Run

Build:
>docker build . -t plexrequests

Run:
>docker run -e MONGO_INITDB_ROOT_USERNAME=username -e MONGO_INITDB_ROOT_PASSWORD=password --name plexrequestsapi plexrequests
