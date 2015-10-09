CREATE TABLE [dbo].[Contacts] (
    [ID]              INT              NOT NULL,
    [FirstName]       VARCHAR (50)     NULL,
    [LastName]        VARCHAR (50)     NULL,
    [PhoneID]         INT              CONSTRAINT [DF_Contacts_PhoneID] DEFAULT ((0)) NULL,
    [CreatedByUserID] INT              CONSTRAINT [DF_Contacts_CreatedByUserID] DEFAULT ((0)) NOT NULL,
    [EditedByUserID]  INT              CONSTRAINT [DF_Contacts_EditedByUserID] DEFAULT ((0)) NOT NULL,
    [Test]            INT              CONSTRAINT [DF_Contacts_Test] DEFAULT ((0)) NOT NULL,
    [TestUnique]      UNIQUEIDENTIFIER CONSTRAINT [DF_Contacts_TestUnique] DEFAULT (newid()) NOT NULL,
    CONSTRAINT [PK_Contacts] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_Contacts_PhoneNumbers] FOREIGN KEY ([PhoneID]) REFERENCES [dbo].[PhoneNumbers] ([ID])
);

