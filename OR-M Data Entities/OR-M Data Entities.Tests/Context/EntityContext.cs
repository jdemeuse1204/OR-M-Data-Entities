using OR_M_Data_Entities.Entity;
using OR_M_Data_Entities.Tests.Tables;

namespace OR_M_Data_Entities.Tests.Context
{
    public class EntityContext : DbEntityContext
    {
        public EntityContext()
            : base("local")
        {
        }

        public IDbTable<Policy> Policies { get; set; } 
    }
}
