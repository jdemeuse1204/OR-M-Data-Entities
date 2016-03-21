CREATE TABLE [dbo].[RecordLabel] (
    [Id]   INT          IDENTITY (1, 1) NOT NULL,
    [Name] VARCHAR (50) NULL,
    CONSTRAINT [PK_RecordLabel] PRIMARY KEY CLUSTERED ([Id] ASC)
);

