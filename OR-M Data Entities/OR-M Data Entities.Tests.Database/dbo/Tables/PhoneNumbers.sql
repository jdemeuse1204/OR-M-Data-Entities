CREATE TABLE [dbo].[PhoneNumbers] (
    [ID]          INT          IDENTITY (1, 1) NOT NULL,
    [Phone]       VARCHAR (20) NULL,
    [PhoneTypeID] INT          CONSTRAINT [DF_PhoneNumbers_PhoneTypeID] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_PhoneNumbers] PRIMARY KEY CLUSTERED ([ID] ASC)
);

