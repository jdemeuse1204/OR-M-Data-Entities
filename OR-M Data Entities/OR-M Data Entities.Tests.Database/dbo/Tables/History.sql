CREATE TABLE [dbo].[History] (
    [Id]          INT          IDENTITY (1, 1) NOT NULL,
    [CreateDate]  ROWVERSION   NOT NULL,
    [Description] VARCHAR (50) NULL,
    [ComputerId]  INT          NOT NULL,
    CONSTRAINT [PK_History] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_History_ComputerId] FOREIGN KEY ([ComputerId]) REFERENCES [dbo].[Computer] ([Id])
);

