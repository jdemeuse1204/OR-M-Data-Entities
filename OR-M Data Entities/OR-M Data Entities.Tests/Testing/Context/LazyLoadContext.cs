using OR_M_Data_Entities.Tests.Testing.Base;

namespace OR_M_Data_Entities.Tests.Testing.Context
{
    public class LazyLoadContext : DbSqlContext
    {
        public LazyLoadContext()
            : base("sqlExpress")
        {
            Configuration.IsLazyLoading = true;
            OnSqlGeneration += ContextMembers.OnOnSqlGeneration;
        }
    }
}
