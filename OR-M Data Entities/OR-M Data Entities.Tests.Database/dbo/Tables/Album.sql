CREATE TABLE [dbo].[Album]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [Name] VARCHAR(50) NULL, 
    [TimesDownloaded] INT NOT NULL, 
    [ArtistId] INT NOT NULL,
	CONSTRAINT [FK_Album_ArtistId] FOREIGN KEY ([ArtistId]) REFERENCES [dbo].[Artist] ([Id])
)
