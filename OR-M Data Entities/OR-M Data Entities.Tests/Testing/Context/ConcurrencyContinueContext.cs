using OR_M_Data_Entities.Enumeration;

namespace OR_M_Data_Entities.Tests.Testing.Context
{
    public class ConcurrencyContinueContext : DbSqlContext
    {
        public ConcurrencyContinueContext()
            : base("sqlExpress")
        {
            Configuration.UseTransactions = false;
            Configuration.ConcurrencyChecking.IsOn = true;
            Configuration.ConcurrencyChecking.ViolationRule = ConcurrencyViolationRule.OverwriteAndContinue;
        }
    }
}
