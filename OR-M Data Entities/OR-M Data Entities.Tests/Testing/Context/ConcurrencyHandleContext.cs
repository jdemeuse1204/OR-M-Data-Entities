using OR_M_Data_Entities.Tests.Testing.Base;

namespace OR_M_Data_Entities.Tests.Testing.Context
{
    public class ConcurrencyHandleContext : DbSqlContext
    {
        public ConcurrencyHandleContext()
            : base("sqlExpress")
        {
            Configuration.UseTransactions = false;
            Configuration.ConcurrencyChecking.IsOn = true;
            Configuration.ConcurrencyChecking.ViolationRule = ConcurrencyViolationRule.UseHandler;
            OnSqlGeneration += ContextMembers.OnOnSqlGeneration;
        }
    }
}
