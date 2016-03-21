-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetLastName2]
(
	-- Add the parameters for the function here
	@Id int
)
RETURNS varchar(100)
AS
BEGIN
	-- Declare the return variable here
	Declare @Result as varchar(100)
	Set @Result = (Select Top 1 LastName From Contacts Where Id = @Id)
	Return @Result
END