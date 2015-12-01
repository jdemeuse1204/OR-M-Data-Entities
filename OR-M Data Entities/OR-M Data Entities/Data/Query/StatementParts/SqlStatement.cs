using System.Data.SqlClient;
using OR_M_Data_Entities.Configuration;
using OR_M_Data_Entities.Data.Secure;

namespace OR_M_Data_Entities.Data.Query.StatementParts
{
    public abstract class SqlStatement : SqlSecureExecutable
    {
        protected SqlStatement(ConfigurationOptions configuration)
        {
            Delcare = string.Empty;
            Set = string.Empty;
            Sql = string.Empty;
            IsTransaction = configuration.UseTransactions;
        }

        public bool IsTransaction { get; private set; }

        public string Delcare { get; set; }

        public string Set { get; set; }

        public string Sql { get; set; }

        public abstract void Build();

        public SqlCommand BuildSqlCommand(SqlConnection connection)
        {
            Build();

            var cmd = new SqlCommand(Sql, connection);

            InsertParameters(cmd);

            return cmd;
        }
    }
}
