using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Collections;
using OR_M_Data_Entities.Expressions.Query;
using OR_M_Data_Entities.Expressions.Resolution.Containers;
using OR_M_Data_Entities.Expressions.Resolution.Join;
using OR_M_Data_Entities.Mapping.Base;

namespace OR_M_Data_Entities.Expressions.Resolution.Select
{
    public abstract class DbSelectQuery<T>
    {
        #region Fields And Properties
        protected readonly SelectInfoResolutionContainer Columns;

        private readonly List<JoinPair> _foreignKeyJoinPairs;
        protected IEnumerable<JoinPair> ForeignKeyJoinPairs { get { return _foreignKeyJoinPairs; } }

        private readonly TableTypeCollection _tables;
        public ReadOnlyTableTypeCollection Tables
        {
            get { return _tables; }
        }

        public ExpressionQueryConstructionType ConstructionType { get; private set; }

        public Guid Id { get; private set; }

        protected readonly Type Type;
        #endregion

        #region Constructor
        protected DbSelectQuery()
        {
            Id = Guid.NewGuid();
            Columns = new SelectInfoResolutionContainer(this.Id);
            _foreignKeyJoinPairs = new List<JoinPair>();
            _tables = new TableTypeCollection { new TableType(typeof(T), this.Id, null, null) };
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

            if (ConstructionType == ExpressionQueryConstructionType.Join)
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
                    .GetValue(query) as List<JoinPair>;

            _tables =
                query.GetType()
                    .GetProperty("Tables", BindingFlags.Public | BindingFlags.Instance)
                    .GetValue(query) as TableTypeCollection;

            Type =
                query.GetType()
                    .GetField("Type", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(query) as Type;

            if (!(this.Type == typeof(T)) && ConstructionType != ExpressionQueryConstructionType.Join)
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
                _tables.Insert(0, new PartialTableType(Type, this.Id, null));
            }

            for (var i = 0; i < _tables.Count; i++)
            {
                var currentType = _tables[i];
                var parentOfCurrent = i == 0 ? null : _foreignKeyJoinPairs.FirstOrDefault(w => w.ChildType == currentType.Type);
                var foreignKeys = DatabaseSchemata.GetAllForeignKeysAndPseudoKeys(currentType.Type);

                if (i == 0)
                {
                    _addPropertiesByType(currentType.Type, currentType.PropertyName, currentType.Alias);
                }
                else
                {
                    _addPropertiesByType(parentOfCurrent.ChildType, parentOfCurrent.ChildPropertyName,
                        parentOfCurrent.ComputedChildAlias);
                }

                // only load base type, this could be a sub query or lazy loading
                if (ConstructionType == ExpressionQueryConstructionType.Join ||
                    ConstructionType == ExpressionQueryConstructionType.SubQuery) break;

                if (foreignKeys.Count == 0) continue;

                _tables.AddRange(
                    foreignKeys.Select(w => new PartialTableType(w.Property.GetPropertyType(), this.Id, w.Property.Name)));

                _foreignKeyJoinPairs.AddRange(
                    foreignKeys.Select(
                        w =>
                            new JoinPair(
                                currentType.Type, w.Property.GetPropertyType(),
                                ((parentOfCurrent != null && parentOfCurrent.HeirarchyContainsList) ||
                                 w.Property.IsPropertyTypeList()),
                                parentOfCurrent == null ? currentType.Alias : parentOfCurrent.ComputedChildAlias,
                                _tables.FindAlias(w.Property.GetPropertyType(), this.Id),
                                w.ParentPropertyName,
                                w.ChildPropertyName,
                                DatabaseSchemata.GetTableName(currentType.Type),
                                DatabaseSchemata.GetTableName(w.Property.GetPropertyType()))
                        ));
            }
        }

        protected void ClearSelectQuery()
        {
            Columns.Clear();
            _foreignKeyJoinPairs.Clear();
        }

        private void _addPropertiesByType(Type type, string foreignKeyTableName, string queryTableName)
        {
            var tableName = DatabaseSchemata.GetTableName(type);

            foreach (var info in type.GetProperties().Where(w => w.GetCustomAttribute<NonSelectableAttribute>() == null))
            {
                Columns.Add(info, type, tableName, queryTableName, foreignKeyTableName, DatabaseSchemata.IsPrimaryKey(type, info.Name));
            }
        }
        #endregion
    }
}
