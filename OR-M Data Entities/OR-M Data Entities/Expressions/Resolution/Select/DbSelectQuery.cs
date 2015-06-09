using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Collections;
using OR_M_Data_Entities.Expressions.Query.Tables;
using OR_M_Data_Entities.Expressions.Resolution.Containers;
using OR_M_Data_Entities.Expressions.Resolution.Join;
using OR_M_Data_Entities.Mapping.Base;

namespace OR_M_Data_Entities.Expressions.Resolution.Select
{
    public abstract class DbSelectQuery<T>
    {
        #region Fields And Properties
        protected readonly SelectInfoResolutionContainer Columns;

        private readonly List<JoinTablePair> _foreignKeyJoinPairs;
        protected IEnumerable<JoinTablePair> ForeignKeyJoinPairs { get { return _foreignKeyJoinPairs; } }

        private readonly TableCollection _tables;
        public ReadOnlyTableCollection Tables
        {
            get { return _tables; }
        }

        public readonly string ViewId;

        public ExpressionQueryConstructionType ConstructionType { get; private set; }

        public Guid Id { get; private set; }

        protected readonly Type Type;
        #endregion

        #region Constructor
        protected DbSelectQuery(string viewId = null)
        {
            ViewId = viewId;
            Id = Guid.NewGuid();
            Columns = new SelectInfoResolutionContainer(this.Id);
            _foreignKeyJoinPairs = new List<JoinTablePair>();
            _tables = new TableCollection { new ForeignKeyTable(this.Id, typeof(T), null) };
            Type = typeof(T);
            ConstructionType = ExpressionQueryConstructionType.Main;

            InitializeSelectInfos();
        }

        protected DbSelectQuery(IExpressionQueryResolvable query, ExpressionQueryConstructionType constructionType)
        {
            ConstructionType = constructionType;
            Id = Guid.NewGuid();

            // never combine columns, selects don't matter, only tables and parameters do for aliasing
            Columns = new SelectInfoResolutionContainer(this.Id);

            if (ConstructionType == ExpressionQueryConstructionType.Join ||
                ConstructionType == ExpressionQueryConstructionType.Select)
            {
                Id = query.Id;
                Columns =
                 query.GetType()
                     .GetField("Columns", BindingFlags.NonPublic | BindingFlags.Instance)
                     .GetValue(query) as SelectInfoResolutionContainer;
            }

            _foreignKeyJoinPairs =
                query.GetType()
                    .GetProperty("ForeignKeyJoinPairs", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(query) as List<JoinTablePair>;

            _tables =
                query.GetType()
                    .GetProperty("Tables", BindingFlags.Public | BindingFlags.Instance)
                    .GetValue(query) as TableCollection;

            Type =
                query.GetType()
                    .GetField("Type", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(query) as Type;

            if (!(this.Type == typeof(T)) && ConstructionType != ExpressionQueryConstructionType.Join &&
                !typeof(T).IsValueType && typeof(T) != typeof(string))
            {
                this.Type = typeof(T);
            }
        }
        #endregion

        #region Methods

        protected void InitializeSelectInfos()
        {
            // if its a subquery the type doesnt exist, we need to add it
            if (ConstructionType == ExpressionQueryConstructionType.SubQuery)
            {
                _tables.Insert(0, new ForeignKeyTable(this.Id, Type, null, null));
            }

            for (var i = 0; i < _tables.Count; i++)
            {
                var currentType = _tables[i];
                var parentOfCurrent = i == 0 ? null : _foreignKeyJoinPairs.FirstOrDefault(w => w.ChildTable.Type == currentType.Type);
                var foreignKeys = DatabaseSchemata.GetAllForeignKeysAndPseudoKeys(currentType.Type, this.Id);

                if (i == 0)
                {
                    _addPropertiesByType(currentType.Type, currentType.ForeignKeyTableName, currentType.Alias);
                }
                else
                {
                    _addPropertiesByType(parentOfCurrent.ChildTable.Type, parentOfCurrent.ChildTable.ForeignKeyTableName,
                        parentOfCurrent.ChildTable.Alias);
                }

                // only load base type, this could be a sub query or lazy loading
                if (ConstructionType == ExpressionQueryConstructionType.Join ||
                    ConstructionType == ExpressionQueryConstructionType.SubQuery) break;

                if (foreignKeys.Count == 0) continue;

                _tables.AddRange(
                    foreignKeys.Select(w => new ForeignKeyTable(this.Id, w.ChildColumn.Property.DeclaringType, w.ChildColumn.PropertyName)));

                _foreignKeyJoinPairs.AddRange(
                    foreignKeys.Select(
                        w =>
                            new JoinTablePair(
                                this.Id,
                                currentType.Type, w.ChildColumn.Property.DeclaringType,
                                ((parentOfCurrent != null && parentOfCurrent.HeirarchyContainsList) || w.JoinType == JoinType.Left),
                                parentOfCurrent == null ? currentType.Alias : parentOfCurrent.ChildTable.Alias,
                                _tables.FindAlias(w.ChildColumn.Table.Type, this.Id),
                                w.ParentColumn.PropertyName,
                                w.ChildColumn.PropertyName)
                        ));
            }
        }

        protected void ClearSelectQuery()
        {
            Columns.Clear();
            _foreignKeyJoinPairs.Clear();
        }

        private void _addPropertiesByType(Type type, string foreignKeyTableName, string alias)
        {
            var tableName = DatabaseSchemata.GetTableName(type);

            foreach (var info in type.GetProperties().Where(w => w.GetCustomAttribute<NonSelectableAttribute>() == null))
            {
                Columns.Add(info, type, tableName, alias, foreignKeyTableName, DatabaseSchemata.IsPrimaryKey(type, info.Name));
            }
        }
        #endregion
    }
}
