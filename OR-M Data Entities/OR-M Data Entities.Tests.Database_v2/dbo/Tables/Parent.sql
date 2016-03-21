CREATE TABLE [dbo].[Parent] (
    [ID]          INT IDENTITY (1, 1) NOT NULL,
    [EditedByID]  INT NOT NULL,
    [CreatedByID] INT NOT NULL,
    CONSTRAINT [PK_Parent] PRIMARY KEY CLUSTERED ([ID] ASC)
);

