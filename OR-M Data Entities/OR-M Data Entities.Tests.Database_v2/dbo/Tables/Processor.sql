CREATE TABLE [dbo].[Processor] (
    [Id]       INT          IDENTITY (1, 1) NOT NULL,
    [Name]     VARCHAR (50) NULL,
    [Cores]    INT          NOT NULL,
    [CoreType] INT          NULL,
    [Speed]    DECIMAL (18) NULL,
    CONSTRAINT [PK_Processor] PRIMARY KEY CLUSTERED ([Id] ASC)
);

