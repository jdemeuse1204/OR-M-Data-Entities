using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities.Tests
{
    public static class Extensions
    {
        public static ConnectionState GetConnectionState(this DbSqlContext context)
        {
            var connectionProperty = typeof(Database).GetProperty("_connection", BindingFlags.Instance | BindingFlags.NonPublic);
            return ((SqlConnection)connectionProperty.GetValue(context)).State;
        }
    }
}
