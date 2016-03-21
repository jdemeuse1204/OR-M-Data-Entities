CREATE TABLE [dbo].[Agent] (
    [Id]   INT          IDENTITY (1, 1) NOT NULL,
    [Name] VARCHAR (50) NULL,
    CONSTRAINT [PK_Agent] PRIMARY KEY CLUSTERED ([Id] ASC)
);

