/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Exceptions;
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

        protected OSchematic QuerySchematic { get; private set; }

        private readonly TableCollection _tables;

        public bool HasForeignKeys {
            get { return _foreignKeyJoinPairs.Count > 0; }
        }

        protected readonly string ViewId;

        protected readonly Type Type;
        #endregion

        #region Constructor
        protected DbSelectQuery(string viewId = null)
        {
            ViewId = viewId;
            Columns = new SelectInfoResolutionContainer(this.Id);
            _foreignKeyJoinPairs = new List<JoinTablePair>();
            _tables = new TableCollection { new ForeignKeyTable(this.Id, typeof(T), null, null) };
            Type = typeof(T);
            ConstructionType = ExpressionQueryConstructionType.Main;

            InitializeQuery();
        }
        #endregion

        #region Methods

        protected void InitializeQuery()
        {
            // if its a subquery the type doesnt exist, we need to add it
            if (ConstructionType == ExpressionQueryConstructionType.SubQuery)
            {
                if (!string.IsNullOrWhiteSpace(this.ViewId) && !Type.IsPartOfView(this.ViewId))
                {
                    throw new ViewException(string.Format("Type Of {0} Does not contain attribute for View - {1}",
                        Type.Name, ViewId));
                }

                _tables.Insert(0, new ForeignKeyTable(this.Id, Type, null, null));
            }

            // setup current schematic
            QuerySchematic = new OSchematic(Type, Type, null);

            var currentSchematic = QuerySchematic;

            for (var i = 0; i < _tables.Count; i++)
            {
                var currentType = _tables[i];
                var parentOfCurrent = i == 0
                    ? null
                    : _foreignKeyJoinPairs.FirstOrDefault(
                        w =>
                            w.ChildTable.Type == currentType.Type &&
                            w.ParentJoinPropertyName == currentType.ForeignKeyPropertyName &&
                            w.ParentTable.Alias == currentType.ParentTableAlias);

                var foreignKeys = currentType.Type.GetAllForeignKeysAndPseudoKeys(this.Id, this.ViewId);

                if (i == 0)
                {
                    _addPropertiesByType(currentType.Type, currentType.ForeignKeyPropertyName, null, currentType.Alias);
                }
                else
                {
                    _addPropertiesByType(parentOfCurrent.ChildTable.Type, parentOfCurrent.ChildTable.ForeignKeyPropertyName,parentOfCurrent.ParentJoinPropertyName,
                        parentOfCurrent.ChildTable.Alias);

                    if (foreignKeys.Count != 0)
                    {
                        currentSchematic = _find(QuerySchematic, currentSchematic, parentOfCurrent.ChildTable.Type,
                            parentOfCurrent.ParentJoinPropertyName);
                    }
                }

                // only load base type, this could be a sub query
                if (ConstructionType == ExpressionQueryConstructionType.Join ||
                    ConstructionType == ExpressionQueryConstructionType.SubQuery) break;

                if (foreignKeys.Count == 0) continue;

                foreach (var joinColumnPair in foreignKeys)
                {
                    currentSchematic.Children.Add(new OSchematic(joinColumnPair.ChildColumn.Property.DeclaringType,
                        joinColumnPair.FromType,
                        joinColumnPair.JoinPropertyName));

                    _tables.Add(new ForeignKeyTable(this.Id, joinColumnPair.ChildColumn.Property.DeclaringType,
                        joinColumnPair.JoinPropertyName, joinColumnPair.JoinPropertyName, currentType.Alias));

                    _foreignKeyJoinPairs.Add(new JoinTablePair(
                        this.Id,

                        currentType.Type, joinColumnPair.ChildColumn.Property.DeclaringType,

                        ((parentOfCurrent != null && parentOfCurrent.HeirarchyContainsList) ||
                         joinColumnPair.JoinType == JoinType.Left),

                        parentOfCurrent == null ? currentType.Alias : parentOfCurrent.ChildTable.Alias,

                        _tables.FindAlias(joinColumnPair.ChildColumn.Table.Type, this.Id,
                            joinColumnPair.JoinPropertyName, currentType.Alias),

                        joinColumnPair.ParentColumn.PropertyName,

                        joinColumnPair.ChildColumn.PropertyName,

                        joinColumnPair.JoinPropertyName));
                }
            }
        }

        /// <summary>
        /// Search through the current schematic to find the reference to the schematic we are looking for.
        /// Do not alter the QuerySchematic!!!!!!!!  Stored reference to it as schematicsToSearch.
        /// </summary>
        /// <param name="beginningSchematic"></param>
        /// <param name="currentSchematic"></param>
        /// <param name="type"></param>
        /// <param name="parentJoinPropertyName"></param>
        /// <returns></returns>
        private OSchematic _find(OSchematic beginningSchematic, OSchematic currentSchematic, Type type, string parentJoinPropertyName)
        {
            var firstSearch =
                        currentSchematic.Children.FirstOrDefault(
                            w =>
                                w.Type == type &&
                                w.PropertyName == parentJoinPropertyName);

            if (firstSearch != null) return firstSearch;

            // do not want a reference to the list and mess up the schematic
            var schematicsToSearch = new List<OSchematic>();

            schematicsToSearch.AddRange(beginningSchematic.Children);

            for (var i = 0; i < schematicsToSearch.Count; i++)
            {
                var schematic = schematicsToSearch[i];

                if (schematic.PropertyName == parentJoinPropertyName) return schematic;

                schematicsToSearch.AddRange(schematic.Children);
            }

            throw new InstanceNotFoundException(string.Format("Cannot find foreign key instance of by name.  NAME: {0}, TYPE: {1}", parentJoinPropertyName, type.Name));
        }

        protected void ClearSelectQuery()
        {
            Columns.Clear();
            _foreignKeyJoinPairs.Clear();
        }

        private void _addPropertiesByType(Type type, string foreignKeyPropertyName, string foreignKeyTableName, string alias)
        {
            var tableName = type.GetTableName();

            foreach (var info in type.GetProperties().Where(w => w.GetCustomAttribute<NonSelectableAttribute>() == null))
            {
                Columns.Add(info, type, tableName, alias, foreignKeyPropertyName, foreignKeyTableName, type.IsPrimaryKey(info.Name));
            }
        }
        #endregion
    }
}
