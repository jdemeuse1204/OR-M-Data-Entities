using System.Data.SqlClient;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Data.Execution;

// ReSharper disable once CheckNamespace
namespace OR_M_Data_Entities
{
    public static class SqlCommandExtensions
    {
        public static PeekDataReader ExecuteReaderWithPeeking(this SqlCommand cmd, SqlConnection connection)
        {
            return new PeekDataReader(cmd, connection);
        }

        public static PeekDataReader ExecuteReaderWithPeeking(this SqlCommand cmd, SqlConnection connection, ISqlPayload payload)
        {
            return new PeekDataReader(cmd, connection, payload);
        }
    }
}
