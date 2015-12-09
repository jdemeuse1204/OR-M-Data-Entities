CREATE TABLE [dbo].[Crust] (
    [Id]        INT          NOT NULL,
    [Name]      VARCHAR (50) NULL,
    [ToppingId] INT          NULL,
    CONSTRAINT [PK_Crust] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Crust_ToppingId] FOREIGN KEY ([ToppingId]) REFERENCES [dbo].[Topping] ([Id])
);

