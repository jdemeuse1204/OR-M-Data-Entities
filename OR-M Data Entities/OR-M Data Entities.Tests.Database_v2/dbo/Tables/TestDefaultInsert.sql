CREATE TABLE [dbo].[TestDefaultInsert] (
    [Id]   INT              IDENTITY (1, 1) NOT NULL,
    [Uid]  UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [Name] VARCHAR (50)     NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

