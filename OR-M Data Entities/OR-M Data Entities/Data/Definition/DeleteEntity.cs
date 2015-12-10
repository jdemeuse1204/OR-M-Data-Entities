using System.Linq;
using OR_M_Data_Entities.Data.Modification;

namespace OR_M_Data_Entities.Data.Definition
{
    public class DeleteEntity : ModificationEntity
    {
        public DeleteEntity(object entity) 
            : base(entity, true)
        {
            ModificationItems = GetColumns().Select(w => new ModificationItem(w)).ToList();
        }
    }
}
