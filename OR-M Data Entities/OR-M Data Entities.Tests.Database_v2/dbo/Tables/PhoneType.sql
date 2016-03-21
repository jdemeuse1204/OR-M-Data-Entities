CREATE TABLE [dbo].[PhoneType] (
    [ID]   INT          IDENTITY (1, 1) NOT NULL,
    [Type] VARCHAR (50) NULL,
    CONSTRAINT [PK_PhoneType] PRIMARY KEY CLUSTERED ([ID] ASC)
);

