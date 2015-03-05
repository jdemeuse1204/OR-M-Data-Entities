/*
 * OR-M Data Entities v1.0.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */
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
