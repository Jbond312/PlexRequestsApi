CREATE TABLE Plex.RequestStatuses
(
	RequestStatusId INT IDENTITY(1,1) NOT NULL,
	Name NVARCHAR(256) NOT NULL,
	CONSTRAINT PK_RequestStatuses PRIMARY KEY (RequestStatusId),
	CONSTRAINT UC_RequestStatuses_Name UNIQUE (Name)
)

CREATE TABLE Plex.AgentTypes
(
	AgentTypeId INT IDENTITY(1,1) NOT NULL,
	Name NVARCHAR(256) NOT NULL,
	CONSTRAINT PK_AgentTypes PRIMARY KEY (AgentTypeId),
	CONSTRAINT UC_AgentTypes_Name UNIQUE (Name)
)

CREATE TABLE Plex.MediaTypes
(
	MediaTypeId INT IDENTITY(1,1) NOT NULL,
	Name NVARCHAR(256) NOT NULL,
	CONSTRAINT PK_MediaTypes PRIMARY KEY (MediaTypeId),
	CONSTRAINT UC_MediaTypes_Name UNIQUE (Name)
)

CREATE TABLE Plex.IssueStatuses
(
	IssueStatusId INT IDENTITY(1,1) NOT NULL,
	Name NVARCHAR(256) NOT NULL,
	CONSTRAINT PK_IssueStatuses PRIMARY KEY (IssueStatusId),
	CONSTRAINT UC_IssueStatuses_Name UNIQUE (Name)
)