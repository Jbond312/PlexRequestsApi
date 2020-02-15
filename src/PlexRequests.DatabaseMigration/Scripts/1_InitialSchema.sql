CREATE TABLE Plex.Users
(
	UserId INT NOT NULL IDENTITY(1,1),
	Identifier UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Users_Identifier DEFAULT (NEWID()),
	PlexAccountId INT NOT NULL,
	Username NVARCHAR(256) NOT NULL,
	Email NVARCHAR(256) NOT NULL,
	IsAdmin BIT NOT NULL CONSTRAINT DF_Users_IsAdmin DEFAULT (0),
	IsDisabled BIT NOT NULL  CONSTRAINT DF_Users_IsDisabled DEFAULT (0),
	LastLoginUTC DATETIME NULL,
	InvalidateTokensBeforeUTC DATETIME NULL,
	CreatedUTC DATETIME NOT NULL CONSTRAINT DF_Users_CreatedUTC DEFAULT(GETUTCDATE()),
	UpdatedUTC DATETIME NULL,
	CONSTRAINT PK_Users PRIMARY KEY (UserId),
	CONSTRAINT UC_Users_Email UNIQUE(Email),
	CONSTRAINT UC_Users_Username UNIQUE(Username),
	CONSTRAINT UC_Users_Identifier UNIQUE(Identifier)
)

CREATE TABLE Plex.UserRoles
(
	UserRoleId INT NOT NULL IDENTITY(1,1),
	UserId INT NOT NULL,
	Role NVARCHAR(256) NOT NULL,
	CreatedUTC DATETIME NOT NULL CONSTRAINT DF_UserRoles_CreatedUTC DEFAULT(GETUTCDATE()),
	UpdatedUTC DATETIME NULL,
	CONSTRAINT PK_UserRoles PRIMARY KEY (UserRoleId),
	CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES Plex.Users (UserId),
	CONSTRAINT UC_UserRoles_UserId_Role UNIQUE(UserId, Role)
)

CREATE TABLE Plex.UserRefreshTokens
(
	UserRefreshTokenId INT NOT NULL IDENTITY(1,1),
	UserId INT NOT NULL,
	Token NVARCHAR(MAX) NOT NULL,
	ExpiresUTC DATETIME NOT NULL,
	CreatedUTC DATETIME NOT NULL CONSTRAINT DF_UserRefreshTokens_CreatedUTC DEFAULT(GETUTCDATE()),
	UpdatedUTC DATETIME NULL,
	CONSTRAINT PK_UserRefreshTokens PRIMARY KEY (UserRefreshTokenId),
	CONSTRAINT FK_UserRefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Plex.Users (UserId)
)

CREATE TABLE Plex.PlexServers
(
	PlexServerId INT NOT NULL IDENTITY(1,1),
	Identifier UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PlexServers_Identifier DEFAULT (NEWID()),
	Name NVARCHAR(256) NOT NULL,
	AccessToken NVARCHAR(2048) NOT NULL,
	MachineIdentifier NVARCHAR(1024) NOT NULL,
	Scheme NVARCHAR(128) NULL,
	LocalIp NVARCHAR(128) NULL,
	LocalPort INT NULL,
	ExternalIp NVARCHAR(128) NULL,
	ExternalPort INT NULL,
	CreatedUTC DATETIME NOT NULL CONSTRAINT DF_PlexServers_CreatedUTC DEFAULT(GETUTCDATE()),
	UpdatedUTC DATETIME NULL,
	CONSTRAINT PK_PlexServers PRIMARY KEY (PlexServerId),
	CONSTRAINT UC_PlexServers_Name UNIQUE(Name),
	CONSTRAINT UC_PlexServers_Identifier UNIQUE(Identifier)
)

CREATE TABLE Plex.PlexLibraries
(
	PlexLibraryId INT NOT NULL IDENTITY(1,1),
	PlexServerId INT NOT NULL,
	LibraryKey NVARCHAR(256) NOT NULL,
	[Type] NVARCHAR(256) NOT NULL,
	Title NVARCHAR(256) NOT NULL,
	IsEnabled BIT NOT NULL CONSTRAINT DF_PlexLibraries_IsEnabled DEFAULT (0),
	IsArchived BIT NOT NULL CONSTRAINT DF_PlexLibraries_IsArchived DEFAULT (0),
	CreatedUTC DATETIME NOT NULL CONSTRAINT DF_PlexLibraries_CreatedUTC DEFAULT(GETUTCDATE()),
	UpdatedUTC DATETIME NULL,
	CONSTRAINT PK_PlexLibraries PRIMARY KEY (PlexLibraryId),
	CONSTRAINT FK_PlexLibraries_PlexServer FOREIGN KEY (PlexServerId) REFERENCES Plex.PlexServers (PlexServerId),
	CONSTRAINT UC_PlexLibraries_LibraryKey UNIQUE(PlexServerId, LibraryKey)
)

CREATE TABLE Plex.PlexMediaItems
(
	PlexMediaItemId INT NOT NULL IDENTITY(1,1),
	Identifier UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PlexMediaItems_Identifier DEFAULT (NEWID()),
	PlexLibraryId INT NOT NULL,
	MediaItemKey INT NOT NULL,
	Title NVARCHAR(256) NOT NULL,
	AgentTypeId INT NOT NULL,
	AgentSourceId NVARCHAR(256),
	MediaUri NVARCHAR(2048) NULL,
	[Year] INT NULL,
	Resolution NVARCHAR(256) NULL,
	MediaTypeId INT NOT NULL,
	CreatedUTC DATETIME NOT NULL CONSTRAINT DF_PlexMediaItems_CreatedUTC DEFAULT(GETUTCDATE()),
	UpdatedUTC DATETIME NULL,
	CONSTRAINT PK_PlexMediaItems PRIMARY KEY (PlexMediaItemId),
	CONSTRAINT FK_PlexMediaItems_PlexLibraries FOREIGN KEY (PlexLibraryId) REFERENCES Plex.PlexLibraries (PlexLibraryId),
	CONSTRAINT FK_PlexMediaItems_AgentTypes FOREIGN KEY (AgentTypeId) REFERENCES Plex.AgentTypes (AgentTypeId),
	CONSTRAINT FK_PlexMediaItems_MediaTypes FOREIGN KEY (MediaTypeId) REFERENCES Plex.MediaTypes (MediaTypeId),
	CONSTRAINT UC_PlexMediaItems_Identifier UNIQUE(Identifier),
	CONSTRAINT UC_PlexMediaItems_Key UNIQUE(PlexLibraryId, MediaItemKey)
)

CREATE TABLE Plex.PlexSeasons
(
	PlexSeasonId INT NOT NULL IDENTITY(1,1),
	Identifier UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PlexSeasons_Identifier DEFAULT (NEWID()),
	PlexMediaItemId INT NOT NULL,
	Season INT NOT NULL,
	MediaItemKey INT NOT NULL,
	Title NVARCHAR(256) NOT NULL,
	AgentTypeId INT NOT NULL,
	AgentSourceId NVARCHAR(256) NULL,
	MediaUri NVARCHAR(2048) NULL,
	CreatedUTC DATETIME NOT NULL CONSTRAINT DF_PlexSeasons_CreatedUTC DEFAULT(GETUTCDATE()),
	UpdatedUTC DATETIME NULL,
	CONSTRAINT PK_PlexSeasons PRIMARY KEY (PlexSeasonId),
	CONSTRAINT FK_PlexSeasons_PlexMediaItems FOREIGN KEY (PlexMediaItemId) REFERENCES Plex.PlexMediaItems (PlexMediaItemId),
	CONSTRAINT FK_PlexSeasons_AgentTypes FOREIGN KEY (AgentTypeId) REFERENCES Plex.AgentTypes (AgentTypeId),
	CONSTRAINT UC_PlexSeasons_Season UNIQUE(PlexMediaItemId, Season),
	CONSTRAINT UC_PlexSeasons_Identifier UNIQUE(Identifier)
)


CREATE TABLE Plex.PlexEpisodes
(
	PlexEpisodeId INT NOT NULL IDENTITY(1,1),
	Identifier UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PlexEpisodes_Identifier DEFAULT (NEWID()),
	PlexSeasonId INT NOT NULL,
	Episode INT NOT NULL,
	Year INT NULL,
	Resolution NVARCHAR(256) NULL,
	MediaItemKey INT NOT NULL,
	Title NVARCHAR(256) NOT NULL,
	AgentTypeId INT NOT NULL,
	AgentSourceId NVARCHAR(256) NULL,
	MediaUri NVARCHAR(2048) NULL,
	CreatedUTC DATETIME NOT NULL CONSTRAINT DF_PlexEpisodes_CreatedUTC DEFAULT(GETUTCDATE()),
	UpdatedUTC DATETIME NULL,
	CONSTRAINT PK_PlexEpisodes PRIMARY KEY (PlexEpisodeId),
	CONSTRAINT FK_PlexEpisodes_PlexSeasons FOREIGN KEY (PlexSeasonId) REFERENCES Plex.PlexSeasons (PlexSeasonId),
	CONSTRAINT FK_PlexEpisodes_AgentTypes FOREIGN KEY (AgentTypeId) REFERENCES Plex.AgentTypes (AgentTypeId),
	CONSTRAINT UC_PlexEpisodes_Episode UNIQUE(PlexSeasonId, Episode),
	CONSTRAINT UC_PlexEpisodes_Identifier UNIQUE(Identifier)
)

CREATE TABLE Plex.MovieRequests
(
	MovieRequestId INT IDENTITY(1,1) NOT NULL,
	TheMovieDbId INT NOT NULL,
	PlexMediaItemId INT NULL,
	UserId INT NOT NULL,
	Title NVARCHAR(256) NOT NULL,
	RequestStatusId INT NOT NULL,
	ImagePath NVARCHAR(2048) NULL,
	AirDateUTC DATETIME NULL,
	Comment NVARCHAR(1024) NULL,
	CreatedUTC DATETIME NOT NULL CONSTRAINT DF_MovieRequests_CreatedUTC DEFAULT(GETUTCDATE()),
	UpdatedUTC DATETIME NULL,
	CONSTRAINT PK_MovieRequests PRIMARY KEY (MovieRequestId),
	CONSTRAINT FK_MovieRequests_Users FOREIGN KEY (UserId) REFERENCES Plex.Users (UserId),
	CONSTRAINT UC_MovieRequests_TheMovieDb_User UNIQUE(TheMovieDbId, UserId)
)

CREATE TABLE Plex.MovieRequestAgents
(
	MovieRequestAgentId INT IDENTITY(1,1) NOT NULL,
	MovieRequestId INT NOT NULL,
	AgentTypeId INT NOT NULL,
	AgentSourceId NVARCHAR(256) NOT NULL,
	CreatedUTC DATETIME NOT NULL CONSTRAINT DF_MovieRequestAgents_CreatedUTC DEFAULT(GETUTCDATE()),
	UpdatedUTC DATETIME NULL,
	CONSTRAINT PK_MovieRequestAgents PRIMARY KEY (MovieRequestAgentId),
	CONSTRAINT FK_MovieRequestAgents_MovieRequests FOREIGN KEY (MovieRequestId) REFERENCES Plex.MovieRequests (MovieRequestId),
	CONSTRAINT FK_MovieRequestAgents_AgentTypes FOREIGN KEY (AgentTypeId) REFERENCES Plex.AgentTypes (AgentTypeId),
	CONSTRAINT UC_MovieRequestAgents_MovieRequest_AgentType UNIQUE(MovieRequestId, AgentTypeId)
)

CREATE TABLE Plex.TvRequests
(
	TvRequestId INT IDENTITY(1,1) NOT NULL,
	TheMovieDbId INT NOT NULL,
	PlexMediaItemId INT NULL,
	Title NVARCHAR(256) NOT NULL,
	RequestStatusId INT NOT NULL,
	ImagePath NVARCHAR(2048) NULL,
	AirDateUTC DATETIME NULL,
	Comment NVARCHAR(1024) NULL,
	Track BIT NOT NULL CONSTRAINT DF_TvRequests_Track DEFAULT (0),
	CreatedUTC DATETIME NOT NULL CONSTRAINT DF_TvRequests_CreatedUTC DEFAULT(GETUTCDATE()),
	UpdatedUTC DATETIME NULL,
	CONSTRAINT PK_TvRequests PRIMARY KEY (TvRequestId),
	CONSTRAINT FK_TvRequests_PlexMediaItems FOREIGN KEY (PlexMediaItemId) REFERENCES Plex.PlexMediaItems (PlexMediaItemId),
	CONSTRAINT FK_TvRequests_RequestStatus FOREIGN KEY (RequestStatusId) REFERENCES Plex.RequestStatuses (RequestStatusId),
	CONSTRAINT UC_TvRequests_TheMovieDb_User UNIQUE(TheMovieDbId, Track)
)

CREATE TABLE Plex.TvRequestAgents
(
	TvRequestAgentId INT IDENTITY(1,1) NOT NULL,
	TvRequestId INT NOT NULL,
	AgentTypeId INT NOT NULL,
	AgentSourceId NVARCHAR(256) NOT NULL,
	CreatedUTC DATETIME NOT NULL CONSTRAINT DF_TvRequestAgents_CreatedUTC DEFAULT(GETUTCDATE()),
	UpdatedUTC DATETIME NULL,
	CONSTRAINT PK_TvRequestAgents PRIMARY KEY (TvRequestAgentId),
	CONSTRAINT FK_TvRequestAgents_TvRequests FOREIGN KEY (TvRequestId) REFERENCES Plex.TvRequests (TvRequestId),
	CONSTRAINT FK_TvRequestAgents_AgentTypes FOREIGN KEY (AgentTypeId) REFERENCES Plex.AgentTypes (AgentTypeId),
	CONSTRAINT UC_TvRequestAgents_TvRequest_AgentType UNIQUE(TvRequestId, AgentTypeId)
)

CREATE TABLE Plex.TvRequestSeasons
(
	TvRequestSeasonId INT IDENTITY(1,1) NOT NULL,
	TvRequestId INT NOT NULL,
	PlexSeasonId INT NULL,
	SeasonIndex INT NOT NULL,
	RequestStatusId INT NOT NULL,
	ImagePath NVARCHAR(2048) NULL,
	AirDateUTC DATETIME NULL,
	CreatedUTC DATETIME NOT NULL CONSTRAINT DF_TvRequestSeasons_CreatedUTC DEFAULT(GETUTCDATE()),
	UpdatedUTC DATETIME NULL,
	CONSTRAINT PK_TvRequestSeasons PRIMARY KEY (TvRequestSeasonId),
	CONSTRAINT FK_TvRequestSeasons_TvRequests FOREIGN KEY (TvRequestId) REFERENCES Plex.TvRequests (TvRequestId),
	CONSTRAINT FK_TvRequestSeasons_PlexMediaSeasons FOREIGN KEY (PlexSeasonId) REFERENCES Plex.PlexSeasons (PlexSeasonId),
	CONSTRAINT FK_TvRequestSeasons_RequestStatus FOREIGN KEY (RequestStatusId) REFERENCES Plex.RequestStatuses (RequestStatusId),
	CONSTRAINT UC_TvRequestSeasons_Season UNIQUE(TvRequestId, SeasonIndex)
)

CREATE TABLE Plex.TvRequestEpisodes
(
	TvRequestEpisodeId INT IDENTITY(1,1) NOT NULL,
	TvRequestSeasonId INT NOT NULL,
	PlexEpisodeId INT NULL,
	EpisodeIndex INT NOT NULL,
	RequestStatusId INT NOT NULL,
	ImagePath NVARCHAR(2048) NULL,
	AirDateUTC DATETIME NULL,
	CreatedUTC DATETIME NOT NULL CONSTRAINT DF_TvRequestEpisodes_CreatedUTC DEFAULT(GETUTCDATE()),
	UpdatedUTC DATETIME NULL,
	CONSTRAINT PK_TvRequestEpisodes PRIMARY KEY (TvRequestEpisodeId),
	CONSTRAINT FK_TvRequestEpisodes_TvRequestSeasons FOREIGN KEY (TvRequestSeasonId) REFERENCES Plex.TvRequestSeasons (TvRequestSeasonId),
	CONSTRAINT FK_TvRequestEpisodes_PlexEpisodes FOREIGN KEY (PlexEpisodeId) REFERENCES Plex.PlexEpisodes (PlexEpisodeId),
	CONSTRAINT FK_TvRequestEpisodes_RequestStatus FOREIGN KEY (RequestStatusId) REFERENCES Plex.RequestStatuses (RequestStatusId),
	CONSTRAINT UC_TvRequestEpisodes_Season UNIQUE(TvRequestSeasonId, EpisodeIndex)
)

CREATE TABLE Plex.TvRequestUsers
(
	TvRequestUserId INT IDENTITY(1,1) NOT NULL,
	TvRequestId INT NOT NULL,
	UserId INT NOT NULL,
	Season INT NULL,
	Episode INT NULL,
	Track INT NOT NULL CONSTRAINT DF_TvRequestUsers_Track DEFAULT (0),
	CreatedUTC DATETIME NOT NULL CONSTRAINT DF_TvRequestUsers_CreatedUTC DEFAULT(GETUTCDATE()),
	UpdatedUTC DATETIME NULL,
	CONSTRAINT PK_TvRequestUsers PRIMARY KEY (TvRequestUserId),
	CONSTRAINT FK_TvRequestUsers_TvRequests FOREIGN KEY (TvRequestId) REFERENCES Plex.TvRequests (TvRequestId),
	CONSTRAINT FK_TvRequestUsers_Users FOREIGN KEY (UserId) REFERENCES Plex.Users (UserId),
	CONSTRAINT UC_TvRequestUsers_SingleRequest UNIQUE(TvRequestId, UserId, Season, Episode)
)

CREATE TABLE Plex.Issues
(
	IssueId INT IDENTITY(1,1) NOT NULL,
	PlexMediaItemId INT NOT NULL,
	UserId INT NOT NULL,
	Title NVARCHAR(256),
	Description NVARCHAR(MAX) NOT NULL,
	CreatedUTC DATETIME NOT NULL CONSTRAINT DF_Issues_CreatedUTC DEFAULT(GETUTCDATE()),
	IssueStatusId INT NOT NULL,
	UpdatedUTC DATETIME NULL,
	CONSTRAINT PK_Issues PRIMARY KEY (IssueId),
	CONSTRAINT FK_Issues_PlexMediaItems FOREIGN KEY (PlexMediaItemId) REFERENCES Plex.PlexMediaItems (PlexMediaItemId),
	CONSTRAINT FK_Issues_Users FOREIGN KEY (UserId) REFERENCES Plex.Users (UserId),
	CONSTRAINT UC_Issue_User_PlexMediaItem UNIQUE (PlexMediaItemId, UserId)
)

CREATE TABLE Plex.IssueComments
(
	IssueCommentId INT IDENTITY(1,1) NOT NULL,
	IssueId INT NOT NULL,
	UserId INT NOT NULL,
	Comment NVARCHAR(MAX),
	LikeCount INT NOT NULL CONSTRAINT DF_IssueComments_LikeCount DEFAULT (0),
	CreatedUTC DATETIME NOT NULL CONSTRAINT DF_IssueComments_CreatedUTC DEFAULT(GETUTCDATE()),
	UpdatedUTC DATETIME NULL,
	CONSTRAINT PK_IssueComments PRIMARY KEY (IssueCommentId),
	CONSTRAINT FK_IssueComments_Issues FOREIGN KEY (IssueId) REFERENCES Plex.Issues (IssueId),
	CONSTRAINT FK_IssueComments_Users FOREIGN KEY (UserId) REFERENCES Plex.Users (UserId),
)
