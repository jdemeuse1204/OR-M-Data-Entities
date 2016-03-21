CREATE TABLE [dbo].[Appointments] (
    [ID]          UNIQUEIDENTIFIER NOT NULL,
    [ContactID]   INT              NOT NULL,
    [Description] VARCHAR (50)     NULL,
    [IsScheduled] BIT              CONSTRAINT [DF_Appointments_IsScheduled] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Appointments] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_Appointments_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ID]),
    CONSTRAINT [FK_Contacts_Table_1] FOREIGN KEY ([ID]) REFERENCES [dbo].[Appointments] ([ID])
);

