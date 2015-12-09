CREATE TABLE [dbo].[PolicyInfo] (
    [Id]          INT              NOT NULL,
    [FirstName]   VARCHAR (50)     NULL,
    [LastName]    VARCHAR (50)     NULL,
    [Description] VARCHAR (50)     NULL,
    [Stamp]       UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_PolicyInfo] PRIMARY KEY CLUSTERED ([Id] ASC)
);

