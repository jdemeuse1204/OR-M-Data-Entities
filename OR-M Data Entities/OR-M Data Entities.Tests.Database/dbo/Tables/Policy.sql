CREATE TABLE [dbo].[Policy] (
    [PolicyID]     INT          IDENTITY (1, 1) NOT NULL,
    [FileNumber]   INT          NOT NULL,
    [PolicyTypeID] INT          NOT NULL,
    [StateID]      INT          NULL,
    [County]       VARCHAR (50) NULL,
    [CreatedDate]  DATETIME     NOT NULL,
    [FeeOwnerName] VARCHAR (50) NULL,
    [InsuredName]  VARCHAR (50) NULL,
    [PolicyAmount] DECIMAL (18) NULL,
    [PolicyDate]   DATETIME     NULL,
    [PolicyNumber] VARCHAR (50) NULL,
    [UpdatedDate]  DATETIME     NOT NULL,
    [CreatedBy]    VARCHAR (50) NULL,
    [UpdatedBy]    VARCHAR (50) NULL,
    CONSTRAINT [PK_Policy] PRIMARY KEY CLUSTERED ([PolicyID] ASC)
);

