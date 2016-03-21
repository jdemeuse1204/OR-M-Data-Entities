CREATE TABLE [dbo].[StateCode] (
    [ID]    INT          IDENTITY (1, 1) NOT NULL,
    [Value] VARCHAR (20) NULL,
    CONSTRAINT [PK_StateCode] PRIMARY KEY CLUSTERED ([ID] ASC)
);

