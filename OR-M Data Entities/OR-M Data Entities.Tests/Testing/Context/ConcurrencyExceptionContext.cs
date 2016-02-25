using System;
using System.Data;
using OR_M_Data_Entities.Tests.Testing.Base;

namespace OR_M_Data_Entities.Tests.Testing.Context
{
    public class ConcurrencyExceptionContext : DbSqlContext
    {
        public ConcurrencyExceptionContext()
            : base("sqlExpress")
        {
            Configuration.UseTransactions = false;
            Configuration.ConcurrencyChecking.IsOn = true;
            Configuration.ConcurrencyChecking.ViolationRule = ConcurrencyViolationRule.ThrowException;
            OnSqlGeneration += ContextMembers.OnOnSqlGeneration;
        }
    }
}
