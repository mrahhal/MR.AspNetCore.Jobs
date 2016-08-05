SET NOCOUNT ON
DECLARE @TARGET_SCHEMA_VERSION INT;
SET @TARGET_SCHEMA_VERSION = 2;

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

	-- DelayedJobs table

	CREATE TABLE [Jobs].[DelayedJobs] (
		[Id]   NVARCHAR (128) NOT NULL,
		[Data] NVARCHAR (MAX) NULL,
		[Due]  DATETIME       NULL,

		CONSTRAINT [PK_Jobs.DelayedJobs] PRIMARY KEY CLUSTERED ([Id] ASC)
	);
	PRINT 'Created table [Jobs].[DelayedJobs]';

	-- CronJobs table

	CREATE TABLE [Jobs].[CronJobs] (
		[Id]       NVARCHAR (128) NOT NULL,
		[Name]     NVARCHAR (128) NOT NULL UNIQUE,
		[TypeName] NVARCHAR (MAX) NULL,
		[Cron]     NVARCHAR (MAX) NULL,
		[LastRun]  DATETIME       NOT NULL,

		CONSTRAINT [PK_Jobs.CronJobs] PRIMARY KEY CLUSTERED ([Id] ASC)
	);
	PRINT 'Created table [Jobs].[DelayedJobs]';

	SET @CURRENT_SCHEMA_VERSION = 1;
END

IF @CURRENT_SCHEMA_VERSION = 1
BEGIN
	PRINT 'Installing schema version 2';

	-- Alter DelayedJobs table

	ALTER TABLE [Jobs].[DelayedJobs] DROP COLUMN [Due]

	ALTER TABLE [Jobs].[DelayedJobs] ADD [Added] DATETIME NOT NULL

	-- DelayedJobDue table

	CREATE TABLE [Jobs].[DelayedJobDue](
		[Id]           INT IDENTITY(1,1) NOT NULL,
		[DelayedJobId] NVARCHAR (128)    NOT NULL,
		[Due]          DATETIME          NULL

		CONSTRAINT [PK_Jobs_DelayedJobDue] PRIMARY KEY CLUSTERED ([Id] ASC)
	);
	PRINT 'Created table [Jobs].[DelayedJobDue]';

	ALTER TABLE [Jobs].[DelayedJobDue] ADD CONSTRAINT [FK_Jobs_DelayedJobDue_DelayedJobs] FOREIGN KEY([DelayedJobId])
		REFERENCES [Jobs].[DelayedJobs] ([Id])
		ON UPDATE CASCADE
		ON DELETE CASCADE;
	PRINT 'Created constraint [FK_Jobs_DelayedJobDue_DelayedJobs]';

	CREATE NONCLUSTERED INDEX [IX_Jobs_DelayedJobDue_JobIdAndDue] ON [Jobs].[DelayedJobDue] (
		[DelayedJobId] ASC,
		[Due]          ASC
	);
	PRINT 'Created index [IX_Jobs_DelayedJobDue_JobIdAndDue]';

	-- DelayedJobParameters table

	CREATE TABLE [Jobs].[DelayedJobParameters](
		[Id]           INT IDENTITY(1,1) NOT NULL,
		[DelayedJobId] NVARCHAR (128)    NOT NULL,
		[Name]         NVARCHAR(40)      NOT NULL,
		[Value]        NVARCHAR(max)     NULL,

		CONSTRAINT [PK_Jobs_JobParameters] PRIMARY KEY CLUSTERED ([Id] ASC)
	);
	PRINT 'Created table [Jobs].[DelayedJobParameters]';

	ALTER TABLE [Jobs].[DelayedJobParameters] ADD CONSTRAINT [FK_Jobs_DelayedJobParameters_DelayedJobs] FOREIGN KEY([DelayedJobId])
		REFERENCES [Jobs].[DelayedJobs] ([Id])
		ON UPDATE CASCADE
		ON DELETE CASCADE;
	PRINT 'Created constraint [FK_Jobs_DelayedJobParameters_DelayedJobs]';

	CREATE NONCLUSTERED INDEX [IX_Jobs_DelayedJobParameters_JobIdAndName] ON [Jobs].[DelayedJobParameters] (
		[DelayedJobId] ASC,
		[Name]         ASC
	);
	PRINT 'Created index [IX_Jobs_DelayedJobParameters_JobIdAndName]';

	SET @CURRENT_SCHEMA_VERSION = 2;
END

UPDATE [Jobs].[Schema] SET [Version] = @CURRENT_SCHEMA_VERSION
IF @@ROWCOUNT = 0
	INSERT INTO [Jobs].[Schema] ([Version]) VALUES (@CURRENT_SCHEMA_VERSION)

PRINT 'Jobs database schema installed';

COMMIT TRANSACTION;
PRINT 'Jobs SQL objects installed';
