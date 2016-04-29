using System.Collections.Generic;
using OR_M_Data_Entities.Configuration;
using OR_M_Data_Entities.Data.Definition;

namespace OR_M_Data_Entities.Mock
{
    public class Database
    {
        public Database()
        {
            _internal = new Dictionary<string, List<object>>();
        }

        private readonly Dictionary<string, List<object>> _internal;

        public void Add(ITableFactory tableFactory, IConfigurationOptions configuration, object entity)
        {
            var table = tableFactory.Find(entity.GetType(), configuration);
            var tableName = table.ToString(TableNameFormat.SqlWithSchema);

            if (!_internal.ContainsKey(tableName)) _internal.Add(tableName, new List<object>());

            _internal[tableName].Add(entity);
        }
    }
}
