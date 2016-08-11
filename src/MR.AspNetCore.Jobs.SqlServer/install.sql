SET NOCOUNT ON
DECLARE @TARGET_SCHEMA_VERSION INT;
SET @TARGET_SCHEMA_VERSION = 1;

PRINT 'Installing Jobs SQL objects...';

BEGIN TRANSACTION;

-- Create the database schema if it doesn't exists
IF NOT EXISTS (SELECT [schema_id] FROM [sys].[schemas] WHERE [name] = 'Jobs')
BEGIN
	EXEC (N'CREATE SCHEMA [Jobs]');
	PRINT 'Created database schema [Jobs]';
END
ELSE
	PRINT 'Database schema [Jobs] already exists';

DECLARE @SCHEMA_ID int;
SELECT @SCHEMA_ID = [schema_id] FROM [sys].[schemas] WHERE [name] = 'Jobs';

-- Create the [Jobs].Schema table if not exists
IF NOT EXISTS(SELECT [object_id] FROM [sys].[tables]
	WHERE [name] = 'Schema' AND [schema_id] = @SCHEMA_ID)
BEGIN
	CREATE TABLE [Jobs].[Schema](
		[Version] [int] NOT NULL,
		CONSTRAINT [PK_Jobs_Schema] PRIMARY KEY CLUSTERED ([Version] ASC)
	);
	PRINT 'Created table [Jobs].[Schema]';
END
ELSE
	PRINT 'Table [Jobs].[Schema] already exists';

DECLARE @CURRENT_SCHEMA_VERSION int;
SELECT @CURRENT_SCHEMA_VERSION = [Version] FROM [Jobs].[Schema];

PRINT 'Current Jobs schema version: ' + CASE @CURRENT_SCHEMA_VERSION WHEN NULL THEN 'none' ELSE CONVERT(nvarchar, @CURRENT_SCHEMA_VERSION) END;

IF @CURRENT_SCHEMA_VERSION IS NOT NULL AND @CURRENT_SCHEMA_VERSION > @TARGET_SCHEMA_VERSION
BEGIN
	ROLLBACK TRANSACTION;
	RAISERROR(N'Current database schema version %d is newer than the configured schema version %d. Please update to the latest package.', 11, 1,
		@CURRENT_SCHEMA_VERSION, @TARGET_SCHEMA_VERSION);
END

IF @CURRENT_SCHEMA_VERSION IS NULL
BEGIN
	PRINT 'Installing schema version 1';

	-- CronJobs table

	CREATE TABLE [Jobs].[CronJobs] (
		[Id]       NVARCHAR (128) NOT NULL,
		[Name]     NVARCHAR (128) NOT NULL UNIQUE,
		[TypeName] NVARCHAR (MAX) NULL,
		[Cron]     NVARCHAR (MAX) NULL,
		[LastRun]  DATETIME       NOT NULL,

		CONSTRAINT [PK_Jobs_CronJobs] PRIMARY KEY CLUSTERED ([Id] ASC)
	);
	PRINT 'Created table [Jobs].[CronJobs]';

	-- Jobs table

	CREATE TABLE [Jobs].[Jobs] (
		[Id]        INT IDENTITY(1,1) NOT NULL,
		[Data]      NVARCHAR (MAX)    NULL,
		[Added]     DATETIME          NOT NULL,
		[Due]       DATETIME          NULL,
		[ExpiresAt] DATETIME          NULL,
		[Retries]   INT               NULL,
		[StateName] NVARCHAR (20)     NOT NULL,

		CONSTRAINT [PK_Jobs_Jobs] PRIMARY KEY CLUSTERED ([Id] ASC)
	);
	PRINT 'Created table [Jobs].[Jobs]';

	CREATE NONCLUSTERED INDEX [IX_Jobs_Jobs_DueAndStateName] ON [Jobs].[Jobs] (
		[Due] ASC,
		[StateName] ASC
	);
	PRINT 'Created index [IX_Jobs_Jobs_Due]';

	CREATE NONCLUSTERED INDEX [IX_Jobs_Jobs_StateName] ON [Jobs].[Jobs] ([StateName] ASC);
	PRINT 'Created index [IX_Jobs_Jobs_StateName]';

	-- JobQueue table

	CREATE TABLE [Jobs].[JobQueue] (
		[Id]    INT IDENTITY(1,1) NOT NULL,
		[JobId] INT               NOT NULL,

		CONSTRAINT [PK_Jobs_JobQueue] PRIMARY KEY CLUSTERED ([Id] ASC)
	);
	PRINT 'Created table [Jobs].[Jobs]';

	SET @CURRENT_SCHEMA_VERSION = 1;
END

UPDATE [Jobs].[Schema] SET [Version] = @CURRENT_SCHEMA_VERSION
IF @@ROWCOUNT = 0
	INSERT INTO [Jobs].[Schema] ([Version]) VALUES (@CURRENT_SCHEMA_VERSION)

PRINT 'Jobs database schema installed';

COMMIT TRANSACTION;
PRINT 'Jobs SQL objects installed';
