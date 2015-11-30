using System.Collections.Generic;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Data.Definition.Base
{
    public class EntityInfoAnalyticsPackage : EntityStateComparePackage
    {
        public EntityInfoAnalyticsPackage(EntityStateComparePackage comparePackage, UpdateType updateType) :
            base(comparePackage.State, comparePackage.ChangeList)
        {
            UpdateType = updateType;
        }

        public EntityInfoAnalyticsPackage(EntityState entityState, IEnumerable<string> changeList, UpdateType updateType) :
            base(entityState, changeList)
        {
            UpdateType = updateType;
        }

        public readonly UpdateType UpdateType;
    }
}
