-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE GetFirstName @Id int
AS
SELECT * 
FROM Contacts
WHERE Id = @Id
