using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Expressions.Query;
using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Expressions.Resolution.Select.Info;

namespace OR_M_Data_Entities.Expressions.Resolution.Containers
{
    public class SelectInfoResolutionContainer : ResolutionContainerBase, IResolutionContainer
    {
        private List<SelectInfo> _infos;
        private List<TableInfo> _tableAliases;

        public bool HasItems
        {
            get
            {
                return _infos != null && _infos.Count > 0;
            }
        }

        public bool IsSelectDistinct { get; set; }

        public int TakeRows { get; set; }

        public IEnumerable<SelectInfo> Infos { get { return _infos; } }

        public IEnumerable<TableInfo> TableAliases { get { return _tableAliases; } }

        public SelectInfoResolutionContainer(Guid expressionQueryId)
            : base(expressionQueryId)
        {
            _infos = new List<SelectInfo>();
            _tableAliases = new List<TableInfo>();
        }

        public void Combine(IResolutionContainer container)
        {
            _tableAliases = container.GetType()
                .GetField("_tableAliases", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(container) as List<TableInfo>;

            _infos = container.GetType()
                .GetField("_infos", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(container) as List<SelectInfo>;
        }

        private SelectInfo _find(PropertyInfo item)
        {
            return _infos.First(w => w.OriginalProperty == item);
        }

        public void Add(PropertyInfo item, Type baseType, string tableName, string queryTableName, string foreignKeyTableName, bool isPrimaryKey)
        {
            if (!_tableAliases.Select(w => w.QueryTableName).Contains(queryTableName))
            {
                _tableAliases.Add(new TableInfo(tableName, foreignKeyTableName, baseType, queryTableName));
            }

            _infos.Add(new SelectInfo(item, baseType, tableName, queryTableName, _infos.Count, isPrimaryKey));
        }

        public string GetNextTableReadName()
        {
            return string.Format("AkA{0}", _tableAliases.Count);
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

            return
                allInfos.Aggregate(result,
                    (current, info) =>
                        current +
                        string.Format("[{0}].[{1}],", info.TableReadName,
                            DatabaseSchemata.GetColumnName(info.OriginalProperty))).TrimEnd(',');
        }

        public string GetOrderStatement()
        {
            var order = _infos.Where(w => w.IsPrimaryKey)
                .OrderBy(w => w.Ordinal)
                .Aggregate(string.Empty, (current, info) => current + string.Format("[{0}].[{1}] ASC,", info.TableReadName,
                    DatabaseSchemata.GetColumnName(info.OriginalProperty))).TrimEnd(',');

            return string.IsNullOrWhiteSpace(order) ? string.Empty : string.Format("ORDER BY {0}", order);
        }
    }
}
