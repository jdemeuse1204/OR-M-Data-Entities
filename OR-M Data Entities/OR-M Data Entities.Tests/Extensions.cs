using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace OR_M_Data_Entities.Tests
{
    public static class Extensions
    {
        public static ConnectionState GetConnectionState(this DbSqlContext context)
        {
            var connectionProperty = context.GetType().GetProperty("Connection", BindingFlags.Instance | BindingFlags.NonPublic);
            return ((SqlConnection)connectionProperty.GetValue(context)).State;
        }
    }
}
