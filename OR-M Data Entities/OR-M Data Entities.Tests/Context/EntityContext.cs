using OR_M_Data_Entities.Entity;

namespace OR_M_Data_Entities.Tests.Context
{
    public class EntityContext : DbEntityContext
    {
        public EntityContext()
            : base("local")
        {
        }
    }
}
