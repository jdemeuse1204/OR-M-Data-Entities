using System;

namespace OR_M_Data_Entities.Tests.Testing.Context
{
    public class InsertKeyChangeContext : DbSqlContext
    {
        public InsertKeyChangeContext()
            : base("sqlExpress")
        {
            Configuration.UseTransactions = false;
            Configuration.InsertKeys.Int = new[] {-1};
            Configuration.InsertKeys.UniqueIdentifier = new[] { Guid.Empty };
        }
    }
}
