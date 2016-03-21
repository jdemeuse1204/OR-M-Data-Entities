CREATE TABLE [dbo].[Album] (
    [Id]              UNIQUEIDENTIFIER NOT NULL,
    [Name]            VARCHAR (50)     NULL,
    [TimesDownloaded] INT              NOT NULL,
    [ArtistId]        INT              NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Album_ArtistId] FOREIGN KEY ([ArtistId]) REFERENCES [dbo].[Artist] ([Id])
);

