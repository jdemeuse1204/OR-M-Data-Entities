CREATE TABLE [dbo].[Pizza] (
    [Id]            INT              IDENTITY (1, 1) NOT NULL,
    [Name]          VARCHAR (100)    NULL,
    [CookTime]      INT              NOT NULL,
    [ToppingId]     INT              CONSTRAINT [DF_Pizza_ToppingId] DEFAULT ((0)) NOT NULL,
    [CrustId]       INT              NOT NULL,
    [DeliveryManId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_Pizza] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_DeliveryMan_DeliveryManId] FOREIGN KEY ([DeliveryManId]) REFERENCES [dbo].[DeliveryMan] ([Id]),
    CONSTRAINT [FK_Pizza_CrustId] FOREIGN KEY ([CrustId]) REFERENCES [dbo].[Crust] ([Id]),
    CONSTRAINT [FK_Topping_ToppingId] FOREIGN KEY ([ToppingId]) REFERENCES [dbo].[Topping] ([Id])
);

