using OR_M_Data_Entities.Tests.Testing.Base;

namespace OR_M_Data_Entities.Tests.Testing.Context
{
    public class TransactionContext : DbSqlContext
    {
        public TransactionContext()
            : base("sqlExpress")
        {
            Configuration.UseTransactions = true;
            OnSqlGeneration += ContextMembers.OnOnSqlGeneration;
        }
    }
}
