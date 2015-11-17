namespace OR_M_Data_Entities.Data.Query.StatementParts
{
    public class SqlTransactionStatement
    {
        public SqlTransactionStatement(string declare, string set, string sql)
        {
            Delcare = declare;
            Set = set;
            Sql = sql;
        }

        public string Delcare { get; private set; }

        public string Set { get; private set; }

        public string Sql { get; private set; }
    }
}
