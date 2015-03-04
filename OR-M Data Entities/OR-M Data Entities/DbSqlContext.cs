using OR_M_Data_Entities.Connection;
using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities
{
    public class DbSqlContext : DataOperations
    {
        #region Constructor
        public DbSqlContext(string connectionString)
            : base(connectionString) { }

        public DbSqlContext(IConnectionBuilder connection)
            : base(connection.BuildConnectionString()) { }
        #endregion
    }
}
