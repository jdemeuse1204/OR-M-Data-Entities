using System;
using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;

namespace OR_M_Data_Entities.Data
{
    public class SaveResult
    {
        public SaveResult(List<KeyValuePair<string, UpdateType>> saves)
        {
            _saves = saves;
        }

        public bool WereAnySkipped
        {
            get { return _saves != null && _saves.Select(w => w.Value).Contains(UpdateType.Skip); }
        }

        private readonly List<KeyValuePair<string, UpdateType>> _saves;

        public UpdateType GetUpdateType(string tableName)
        {
            if (_saves == null || !_saves.Select(w => w.Key).Contains(tableName))
            {
                throw new Exception("Table name not found among save collection");
            }

            return _saves.First(w => w.Key == tableName).Value;
        }

        public UpdateType GetUpdateType(Type type)
        {
            var table = new Table(type);

            return GetUpdateType(table.TableNameOnly);
        }
    }
}
