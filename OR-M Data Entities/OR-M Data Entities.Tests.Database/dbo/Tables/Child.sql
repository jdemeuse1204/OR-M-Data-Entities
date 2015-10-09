CREATE TABLE [dbo].[Child] (
    [ID]   INT          IDENTITY (1, 1) NOT NULL,
    [Name] VARCHAR (50) NULL,
    CONSTRAINT [PK_Child] PRIMARY KEY CLUSTERED ([ID] ASC)
);

