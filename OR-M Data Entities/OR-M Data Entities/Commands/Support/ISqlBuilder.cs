using System.Data.SqlClient;

namespace OR_M_Data_Entities.Commands.Support
{
	public interface ISqlBuilder
	{
		void Table(string tableName);
		SqlCommand Build(SqlConnection connection);
	}
}
