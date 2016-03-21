CREATE TABLE [dbo].[Person] (
    [ID]            INT          IDENTITY (1, 1) NOT NULL,
    [FirstName]     VARCHAR (50) NULL,
    [LastName]      VARCHAR (50) NULL,
    [StreetAddress] VARCHAR (50) NULL,
    [City]          VARCHAR (50) NULL,
    [State]         VARCHAR (50) NULL,
    [Zip]           VARCHAR (5)  NULL,
    [CarID]         INT          NOT NULL,
    CONSTRAINT [PK_Person] PRIMARY KEY CLUSTERED ([ID] ASC)
);

