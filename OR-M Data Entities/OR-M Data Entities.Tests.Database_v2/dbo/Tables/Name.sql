CREATE TABLE [dbo].[Name] (
    [ID]        INT          IDENTITY (1, 1) NOT NULL,
    [Value]     VARCHAR (50) NULL,
    [ContactID] INT          NOT NULL,
    CONSTRAINT [PK_Name] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_Name_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ID])
);

