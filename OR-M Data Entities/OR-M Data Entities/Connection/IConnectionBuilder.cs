/*
 * OR-M Data Entities v1.0.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */
namespace OR_M_Data_Entities.Connection
{
	public interface IConnectionBuilder
	{
		string Username { get; set; }
		string Password { get; set; }

		string BuildConnectionString();
	}
}
