namespace OR_M_Data_Entities.Tests.Testing.Context
{
    public class DefaultContext : DbSqlContext
    {
        public DefaultContext()
            : base("sqlExpress")
        {
            Configuration.UseTransactions = false;
            Configuration.ConcurrencyChecking.IsOn = false;
        }
    }
}
