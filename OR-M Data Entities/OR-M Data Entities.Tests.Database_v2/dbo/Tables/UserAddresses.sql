CREATE TABLE [dbo].[UserAddresses] (
    [AddressId] INT NOT NULL,
    [UserId]    INT NOT NULL,
    CONSTRAINT [PK_UserAddresses] PRIMARY KEY CLUSTERED ([AddressId] ASC, [UserId] ASC)
);

