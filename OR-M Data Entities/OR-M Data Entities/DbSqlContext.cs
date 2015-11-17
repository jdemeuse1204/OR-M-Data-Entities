/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */
using System.Data.SqlClient;
using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities
{
    public class DbSqlContext : DatabaseModifiable
    {
        #region Constructor
        public DbSqlContext(string connectionStringOrName)
            : base(connectionStringOrName) { }

        public DbSqlContext(SqlConnectionStringBuilder connection)
            : this(connection.ConnectionString) { }
        #endregion
    }
}
