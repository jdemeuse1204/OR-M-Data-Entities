using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Expressions.Collections;
using OR_M_Data_Entities.Expressions.Query;
using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Expressions.Resolution.Containers;
using OR_M_Data_Entities.Expressions.Resolution.Join;
using OR_M_Data_Entities.Mapping.Base;

namespace OR_M_Data_Entities.Expressions.Resolution.Select
{
    public abstract class DbSelectQuery<T>
    {
        #region Fields And Properties
        protected readonly SelectInfoResolutionContainer SelectList;

        private readonly List<JoinPair> _foreignKeyJoinPairs;
        protected IEnumerable<JoinPair> ForeignKeyJoinPairs { get { return _foreignKeyJoinPairs; } }

        private readonly TableTypeCollection _tables;
        protected ReadOnlyTableTypeCollection Tables
        {
            get { return _tables; }
        }
        #endregion

        #region Constructor
        protected DbSelectQuery(QueryInitializerType queryInitializerType)
        {
            SelectList = new SelectInfoResolutionContainer();
            _foreignKeyJoinPairs = new List<JoinPair>();
            _tables = new TableTypeCollection {new TableType(typeof (T), null, null)};

            _initialize(queryInitializerType);
        }

        protected DbSelectQuery(IExpressionQueryResolvable query)
        {
            SelectList =
                query.GetType()
                    .GetField("SelectList", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(query) as SelectInfoResolutionContainer;

            _foreignKeyJoinPairs =
                query.GetType()
                    .GetProperty("ForeignKeyJoinPairs", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(query) as List<JoinPair>;

            _tables =
                query.GetType()
                    .GetProperty("Tables", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(query) as TableTypeCollection;
        }
        #endregion

        #region Methods

        private void _initialize(QueryInitializerType queryInitializerType)
        {
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
                if (queryInitializerType == QueryInitializerType.None) break;

                if (foreignKeys.Count == 0) continue;

                _tables.AddRange(
                    foreignKeys.Select(w => new PartialTableType(w.Property.GetPropertyType(), w.Property.Name)));

                _foreignKeyJoinPairs.AddRange(
                    foreignKeys.Select(
                        w =>
                            new JoinPair(
                                currentType.Type, w.Property.GetPropertyType(),
                                ((parentOfCurrent != null && parentOfCurrent.HeirarchyContainsList) ||
                                 w.Property.IsPropertyTypeList()),
                                parentOfCurrent == null ? currentType.Alias : parentOfCurrent.ComputedChildAlias,
                                _tables.FindAlias(w.Property.GetPropertyType()),
                                w.ParentPropertyName,
                                w.ChildPropertyName,
                                DatabaseSchemata.GetTableName(currentType.Type),
                                DatabaseSchemata.GetTableName(w.Property.GetPropertyType()))
                        ));
            }
        }

        private void _addPropertiesByType(Type type, string foreignKeyTableName, string queryTableName)
        {
            var tableName = DatabaseSchemata.GetTableName(type);

            foreach (var info in type.GetProperties().Where(w => w.GetCustomAttribute<NonSelectableAttribute>() == null))
            {
                SelectList.Add(info, type, tableName, queryTableName, foreignKeyTableName, DatabaseSchemata.IsPrimaryKey(type, info.Name));
            }
        }
        #endregion
    }
}
