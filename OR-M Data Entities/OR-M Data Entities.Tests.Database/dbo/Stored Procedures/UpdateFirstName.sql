-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
Create PROCEDURE [dbo].[UpdateFirstName] @Id int, @FirstName as varchar(100)
AS
Update Contacts
Set FirstName = @FirstName
WHERE Id = @Id
