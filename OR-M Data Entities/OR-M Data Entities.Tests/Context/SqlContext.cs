namespace OR_M_Data_Entities.Tests.Context
{
    public class SqlContext : DbSqlContext
    {
        public SqlContext() 
            : base("local")
        {
        }
    }
}
