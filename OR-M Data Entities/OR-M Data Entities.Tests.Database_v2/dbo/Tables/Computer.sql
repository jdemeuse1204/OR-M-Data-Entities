CREATE TABLE [dbo].[Computer] (
    [Id]          INT          IDENTITY (1, 1) NOT NULL,
    [Name]        VARCHAR (50) NULL,
    [ProcessorId] INT          NOT NULL,
    [IsCustom]    INT          NOT NULL,
    CONSTRAINT [PK_Computer] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Comuputer_ProcessorId] FOREIGN KEY ([ProcessorId]) REFERENCES [dbo].[Processor] ([Id])
);

