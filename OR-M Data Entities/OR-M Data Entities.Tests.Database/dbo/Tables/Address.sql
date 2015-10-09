CREATE TABLE [dbo].[Address] (
    [ID]            INT              IDENTITY (1, 1) NOT NULL,
    [Addy]          VARCHAR (100)    NULL,
    [AppointmentID] UNIQUEIDENTIFIER NOT NULL,
    [StateID]       INT              NOT NULL,
    CONSTRAINT [PK_Address] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_Address_Appointments] FOREIGN KEY ([AppointmentID]) REFERENCES [dbo].[Appointments] ([ID]),
    CONSTRAINT [FK_Address_StateCode] FOREIGN KEY ([StateID]) REFERENCES [dbo].[StateCode] ([ID])
);

