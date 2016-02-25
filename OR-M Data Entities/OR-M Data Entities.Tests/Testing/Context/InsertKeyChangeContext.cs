using System;
using OR_M_Data_Entities.Tests.Testing.Base;

namespace OR_M_Data_Entities.Tests.Testing.Context
{
    public class InsertKeyChangeContext : DbSqlContext
    {
        public InsertKeyChangeContext()
            : base("sqlExpress")
        {
            Configuration.UseTransactions = false;

            // note this only works when the keys are automatically generated
            Configuration.InsertKeys.Int32 = new [] {-1};
            Configuration.InsertKeys.Guid = new[] { Guid.Empty };
            Configuration.InsertKeys.String = new[] {string.Empty};

            OnSqlGeneration += ContextMembers.OnOnSqlGeneration;
        }
    }
}
