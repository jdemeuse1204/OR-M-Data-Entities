using OR_M_Data_Entities.Tests.Testing.Base;

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
            OnSqlGeneration += ContextMembers.OnOnSqlGeneration;
        }
    }
}
