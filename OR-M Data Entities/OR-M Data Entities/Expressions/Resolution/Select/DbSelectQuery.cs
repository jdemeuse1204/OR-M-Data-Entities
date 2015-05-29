using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;
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
        public readonly SelectInfoResolutionContainer SelectList;


        private readonly List<JoinPair> _foreignKeyJoinPairs;
        protected IEnumerable<JoinPair> ForeignKeyJoinPairs { get { return _foreignKeyJoinPairs; } } 

        private readonly TableTypeCollection _tables;
        protected ReadOnlyTableTypeCollection Tables {
            get { return _tables; }
        }
        #endregion

        #region Constructor
        protected DbSelectQuery(QueryInitializerType queryInitializerType)
        {
            SelectList = new SelectInfoResolutionContainer();
            _foreignKeyJoinPairs = new List<JoinPair>();
            _tables = new TableTypeCollection {typeof (T)};

            if (queryInitializerType == QueryInitializerType.WithForeignKeys)
            {
                _initializeTypesWithForeignKeys();
            }
        }
        #endregion

        #region Methods

        private void _initializeTypesWithForeignKeys()
        {
            for (var i = 0; i < _tables.Count; i++)
            {
                var currentType = _tables[i];
                var parentOfCurrent = i == 0 ? null : _foreignKeyJoinPairs.FirstOrDefault(w => w.ChildType == currentType.Type);
                var foreignKeys = DatabaseSchemata.GetAllForeignKeysAndPseudoKeys(currentType.Type);

                if (foreignKeys.Count == 0) continue;

                _tables.AddRange(foreignKeys.Select(w => w.GetPropertyType()));

                _foreignKeyJoinPairs.AddRange(
                    foreignKeys.Select(
                        w =>
                            new JoinPair(
                                currentType.Type, w.GetPropertyType(),
                                ((parentOfCurrent != null && parentOfCurrent.HeirarchyContainsList) ||
                                 w.IsPropertyTypeList()),
                                parentOfCurrent == null ? currentType.Alias : parentOfCurrent.ComputedChildAlias,
                                _tables.FindAlias(w.GetPropertyType()),
                                w.Name,
                                DatabaseSchemata.GetTableName(currentType.Type),
                                DatabaseSchemata.GetTableName(w.GetPropertyType()))
                        ));
            }
        }

        private void _addPropertiesByType(Type type, string foreignKeyTableName, string queryTableName)
        {
            var tableName = DatabaseSchemata.GetTableName(type);

            foreach (var info in type.GetProperties().Where(w => w.GetCustomAttribute<NonSelectableAttribute>() == null))
            {
                SelectList.Add(info, type, tableName, queryTableName, foreignKeyTableName);
            }
        }
        #endregion
    }
}
