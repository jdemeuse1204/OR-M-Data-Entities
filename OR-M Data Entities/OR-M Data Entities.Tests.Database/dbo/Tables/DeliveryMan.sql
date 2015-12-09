CREATE TABLE [dbo].[DeliveryMan] (
    [Id]                  UNIQUEIDENTIFIER NOT NULL,
    [FirstName]           VARCHAR (50)     NULL,
    [LastName]            VARCHAR (50)     NULL,
    [AverageDeliveryTime] INT              NOT NULL,
    [CreateDate]          ROWVERSION       NOT NULL,
    CONSTRAINT [PK_DeliveryMan] PRIMARY KEY CLUSTERED ([Id] ASC)
);

