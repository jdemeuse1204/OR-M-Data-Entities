-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetLastName]
(
	-- Add the parameters for the function here
	@Id int,
	@FirstName varchar(100)
)
RETURNS varchar(100)
AS
BEGIN
	-- Declare the return variable here
	Declare @Result as varchar(100)
	Set @Result = (Select Top 1 LastName From Contacts Where Id = @Id And FirstName = @FirstName)
	Return @Result
END