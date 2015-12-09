CREATE TABLE [dbo].[ZipCode] (
    [ID]        INT         IDENTITY (1, 1) NOT NULL,
    [Zip5]      VARCHAR (5) NULL,
    [Zip4]      VARCHAR (4) NULL,
    [AddressID] INT         NOT NULL,
    CONSTRAINT [PK_ZipCode] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_ZipCode_Address] FOREIGN KEY ([AddressID]) REFERENCES [dbo].[Address] ([ID])
);

