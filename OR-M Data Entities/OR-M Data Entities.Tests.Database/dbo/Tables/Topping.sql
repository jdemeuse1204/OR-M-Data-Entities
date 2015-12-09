CREATE TABLE [dbo].[Topping] (
    [Id]   INT          IDENTITY (1, 1) NOT NULL,
    [Cost] DECIMAL (18) NOT NULL,
    [Name] VARCHAR (50) NULL,
    CONSTRAINT [PK_Topping] PRIMARY KEY CLUSTERED ([Id] ASC)
);

