CREATE TABLE [dbo].[Linking] (
    [PolicyId]     INT          NOT NULL,
    [PolicyInfoId] INT          NOT NULL,
    [Description]  VARCHAR (50) NULL,
    CONSTRAINT [PK_Linking] PRIMARY KEY CLUSTERED ([PolicyId] ASC, [PolicyInfoId] ASC)
);

