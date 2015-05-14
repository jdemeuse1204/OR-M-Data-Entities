using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Expressions.Resolution.Base;

namespace OR_M_Data_Entities.Expressions.Resolution.Containers
{
    public class SelectInfoResolutionContainer : IResolutionContainer
    {
        private readonly List<SelectInfo> _infos;

        public bool HasItems
        {
            get
            {
                return _infos != null && _infos.Count > 0;
            }
        }

        public IEnumerable<SelectInfo> Infos { get { return _infos; } }

        public SelectInfoResolutionContainer()
        {
            _infos = new List<SelectInfo>();
        }

        private SelectInfo _find(PropertyInfo item)
        {
            return _infos.First(w => w.OriginalProperty == item);
        }


        public void Add(PropertyInfo item, Type baseType, string tableName)
        {
            _infos.Add(new SelectInfo(item, baseType, tableName, _infos.Count));
        }

        public void UnSelectAll()
        {
            foreach (var info in _infos)
            {
                info.IsSelected = false;
            }

            ReturnPropertyOnly = false;
        }

        public bool ReturnPropertyOnly { get; set; }

        public void CopyTo(SelectInfo[] array, int arrayIndex)
        {
            _infos.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _infos.Count; }
        }
        public bool IsReadOnly { get; private set; }

        public void Remove(PropertyInfo item)
        {
            _infos.Remove(_find(item));
        }

        public void Clear()
        {
            _infos.Clear();
        }

        public bool Contains(PropertyInfo item)
        {
            return _infos.Contains(_find(item));
        }

        public int IndexOf(PropertyInfo item)
        {
            return _infos.IndexOf(_find(item));
        }

        public void Insert(int index, PropertyInfo item)
        {
            _infos.Insert(index, _find(item));
        }

        public void RemoveAt(int index)
        {
            _infos.RemoveAt(index);
        }

        public SelectInfo this[int index]
        {
            get { return _infos[index]; }
            set { _infos[index] = value; }
        }

        public string Resolve()
        {
            var result = string.Empty;
            var allUnselected = !_infos.Any(w => w.IsSelected);
            var allInfos = allUnselected ? _infos : _infos.Where(w => w.IsSelected);

            return allInfos.Aggregate(result, (current, info) => current + string.Format("[{0}].[{1}],", info.TableName, DatabaseSchemata.GetColumnName(info.OriginalProperty))).TrimEnd(',');
        }
    }
}
