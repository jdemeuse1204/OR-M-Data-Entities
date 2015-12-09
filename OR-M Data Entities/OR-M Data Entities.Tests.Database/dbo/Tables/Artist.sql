CREATE TABLE [dbo].[Artist]
(
	[Id] INT IDENTITY (1, 1) NOT NULL, 
    [FirstName] VARCHAR(75) NULL, 
    [LastName] VARCHAR(75) NULL, 
    [Genre] VARCHAR(75) NULL, 
    [ActiveDate] DATETIME NOT NULL,
	[RecordLabelId] INT NULL, 
	[AgentId] INT NOT NULL,
    CONSTRAINT [PK_Artist] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_Artist_RecordLabelId] FOREIGN KEY ([RecordLabelId]) REFERENCES [dbo].[RecordLabel] ([Id]),
	CONSTRAINT [FK_Artist_AgentId] FOREIGN KEY ([AgentId]) REFERENCES [dbo].[Agent] ([Id])
)
