CREATE TABLE [dbo].[TryInsertWithGeneration]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [SequenceNumber] INT NOT NULL, 
    [OtherNumber] INT NOT NULL IDENTITY, 
    [Name] VARCHAR(50) NULL
)
