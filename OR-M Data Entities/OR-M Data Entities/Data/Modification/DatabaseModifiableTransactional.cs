using System;
using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Data.Query;
using OR_M_Data_Entities.Enumeration;

namespace OR_M_Data_Entities.Data
{
    public partial class DatabaseModifiable
    {
        private class Reference
        {
             
        }

        private void _buildReferenceMap(object entity)
        {
            
        }

        public virtual bool _saveChangesUsingTransactions<T>(T entity)
            where T : class
        {
            var saves = new List<UpdateType>();
            var parent = new ModificationEntity(entity);
            var builders = new List<ISqlBuilder>();

            // get all items to save and get them in order
            var entityItems = _getSaveItems(parent);

            for (var i = 0; i < entityItems.Count; i++)
            {
                var entityItem = entityItems[i];
                ISqlBuilder builder;

                // add the save to the list so we can tell the user what the save action did
                saves.Add(entityItem.Entity.UpdateType);

                if (OnBeforeSave != null) OnBeforeSave(entityItem.Entity.Value, entityItem.Entity.UpdateType);

                // Get the correct execution plan
                switch (entityItem.Entity.UpdateType)
                {
                    case UpdateType.Insert:
                        builder = new SqlInsertBuilder(entityItem.Entity);
                        break;
                    case UpdateType.TryInsert:
                        builder = new SqlTryInsertBuilder(entityItem.Entity);
                        break;
                    case UpdateType.TryInsertUpdate:
                        builder = new SqlTryInsertUpdateBuilder(entityItem.Entity);
                        break;
                    case UpdateType.Update:
                        builder = new SqlUpdateBuilder(entityItem.Entity);
                        break;
                    case UpdateType.Skip:
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                builders.Add(builder);

                // execute transaction
                // make a transaction builder because they will be so different
            }

            return saves.Any(w => w != UpdateType.Skip);
        }
    }
}
