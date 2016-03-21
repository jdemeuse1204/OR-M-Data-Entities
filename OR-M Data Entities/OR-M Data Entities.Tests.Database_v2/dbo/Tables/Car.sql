CREATE TABLE [dbo].[Car] (
    [ID]         INT          IDENTITY (1, 1) NOT NULL,
    [Name]       VARCHAR (50) NULL,
    [Make]       VARCHAR (50) NULL,
    [Model]      VARCHAR (50) NULL,
    [Trim]       VARCHAR (50) NULL,
    [Horsepower] INT          NOT NULL,
    CONSTRAINT [PK_Car] PRIMARY KEY CLUSTERED ([ID] ASC)
);

