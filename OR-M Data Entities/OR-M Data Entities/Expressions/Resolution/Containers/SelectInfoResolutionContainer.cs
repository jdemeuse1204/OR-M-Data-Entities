using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Expressions.Query.Columns;
using OR_M_Data_Entities.Expressions.Query.Tables;
using OR_M_Data_Entities.Expressions.Resolution.Base;

namespace OR_M_Data_Entities.Expressions.Resolution.Containers
{
    public class SelectInfoResolutionContainer : ResolutionContainerBase, IResolutionContainer
    {
        private List<DbColumn> _infos;
        private List<ForeignKeyTable> _tableAliases;

        public bool HasItems
        {
            get
            {
                return _infos != null && _infos.Count > 0;
            }
        }

        public bool IsSelectDistinct { get; set; }

        public int TakeRows { get; set; }

        public IEnumerable<DbColumn> Infos { get { return _infos; } }

        public IEnumerable<ForeignKeyTable> TableAliases { get { return _tableAliases; } }

        public SelectInfoResolutionContainer(Guid expressionQueryId)
            : base(expressionQueryId)
        {
            _infos = new List<DbColumn>();
            _tableAliases = new List<ForeignKeyTable>();
        }

        public void Combine(IResolutionContainer container)
        {
            _tableAliases = container.GetType()
                .GetField("_tableAliases", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(container) as List<ForeignKeyTable>;

            _infos = container.GetType()
                .GetField("_infos", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(container) as List<DbColumn>;
        }

        private DbColumn _find(PropertyInfo item)
        {
            return _infos.First(w => w.Property == item);
        }

        public void Add(PropertyInfo item, Type baseType, string tableName, string tableAlias, string foreignKeyTableName, bool isPrimaryKey)
        {
            if (!_tableAliases.Select(w => w.Alias).Contains(tableAlias))
            {
                _tableAliases.Add(new ForeignKeyTable(this.ExpressionQueryId, baseType, foreignKeyTableName, tableAlias));
            }

            _infos.Add(new DbColumn(this.ExpressionQueryId, item.DeclaringType, item, tableAlias, isPrimaryKey,
                _infos.Count));
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

        public void CopyTo(DbColumn[] array, int arrayIndex)
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

        public DbColumn this[int index]
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
                        string.Format("[{0}].[{1}],", info.GetTableAlias(),
                            DatabaseSchemata.GetColumnName(info.Property))).TrimEnd(',');
        }

        public string GetOrderStatement()
        {
            var order = _infos.Where(w => w.IsPrimaryKey)
                .OrderBy(w => w.Ordinal)
                .Aggregate(string.Empty, (current, info) => current + string.Format("[{0}].[{1}] ASC,", info.GetTableAlias(),
                    DatabaseSchemata.GetColumnName(info.Property))).TrimEnd(',');

            return string.IsNullOrWhiteSpace(order) ? string.Empty : string.Format("ORDER BY {0}", order);
        }
    }
}
