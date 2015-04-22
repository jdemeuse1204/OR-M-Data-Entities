using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions.ObjectMapping.Base;

namespace OR_M_Data_Entities.Mapping.Objects
{
    /// <summary>
    /// Only used by expression queries
    /// </summary>
    public class ObjectMapper
    {
        #region Constructor
        public ObjectMapper(DataFetching context)
        {
            IsLazyLoadingEnabled = context.IsLazyLoadEnabled;
            SchemaName = context.SchemaName;

            if (IsLazyLoadingEnabled)
            {
                MapType = ObjectMapReturnType.Lazy;
            }
        }
        #endregion

        #region Properties
        public string SchemaName { get; private set; }

        public bool IsLazyLoadingEnabled { get; private set; }

        public ObjectMapReturnType MapType { get; private set; }
        #endregion

        public void MapSelectAll<T>(T entity)
        {
            if (MapType == ObjectMapReturnType.Lazy) return;

            if (DatabaseSchemata.HasForeignKeys(entity))
            {
                
            }
            // when columns are created push in the table name
            // columns should be referenced by property name
        }

        public void MapSelect<T>(T entity)
        {
            if (MapType == ObjectMapReturnType.Lazy) return;

            if (DatabaseSchemata.HasForeignKeys(entity))
            {

            }
        }
    }
}
