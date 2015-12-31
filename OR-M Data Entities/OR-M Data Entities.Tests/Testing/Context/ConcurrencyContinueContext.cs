using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Scripts.Base;
using OR_M_Data_Entities.Tests.Scripts;
using OR_M_Data_Entities.Tests.Tables;

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
