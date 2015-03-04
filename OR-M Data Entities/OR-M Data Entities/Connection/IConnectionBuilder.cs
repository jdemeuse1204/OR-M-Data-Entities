namespace OR_M_Data_Entities.Connection
{
	public interface IConnectionBuilder
	{
		string Username { get; set; }
		string Password { get; set; }

		string BuildConnectionString();
	}
}
