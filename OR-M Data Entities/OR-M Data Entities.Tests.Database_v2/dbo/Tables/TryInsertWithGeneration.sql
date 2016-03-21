CREATE TABLE [dbo].[TryInsertWithGeneration] (
    [Id]             INT          NOT NULL,
    [SequenceNumber] INT          NOT NULL,
    [OtherNumber]    INT          IDENTITY (1, 1) NOT NULL,
    [Name]           VARCHAR (50) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

