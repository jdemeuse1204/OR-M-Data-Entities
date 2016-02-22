using System;

namespace OR_M_Data_Entities.Tests.Testing.Context
{
    public class InsertKeyChangeContext : DbSqlContext
    {
        public InsertKeyChangeContext()
            : base("sqlExpress")
        {
            Configuration.UseTransactions = false;
            Configuration.InsertKeys.Int16 = new short[] {-1};
            Configuration.InsertKeys.Guid = new[] { Guid.Empty };
        }
    }
}
