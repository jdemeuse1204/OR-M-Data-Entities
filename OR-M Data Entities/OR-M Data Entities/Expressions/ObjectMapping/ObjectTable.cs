/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions.Containers;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Expressions.ObjectMapping
{
    public class ObjectTable : IEquatable<ObjectTable>
    {
        #region Constructor
        public ObjectTable(Type type, string alias, string tableName, bool isBaseTable = false, bool selectAllColumns = true)
        {
            LinkedServer = type.GetCustomAttribute<LinkedServerAttribute>();
            var i = -1;
            Alias = alias;
            TableName = tableName;
            Columns =
                DatabaseSchemata.GetTableFields(type)
                    .Select(
                        w =>
                            new ObjectColumn(w,
                                LinkedServer != null
                                    ? string.Format("{0}.[{1}]", LinkedServer.LinkedServerText, tableName)
                                    : string.Empty, ++i, tableName, alias, selectAllColumns))
                    .ToList();
            Type = type;
            IsBaseTable = isBaseTable;
            LinkedServer = type.GetCustomAttribute<LinkedServerAttribute>();
        }
        #endregion

        #region Properties
        public Type Type { get; set; }

        public string Alias { get; set; }

        public string TableName { get; set; }

        public bool IsBaseTable { get; private set; }

        public List<ObjectColumn> Columns { get; set; }

        public LinkedServerAttribute LinkedServer { get; private set; }

        public bool HasAlias { get { return !TableName.Equals(Alias); } }

        public bool HasOrderSequence()
        {
            return Columns != null && Columns.Any(w => w.HasOrderSequence);
        }

        public void OrderByPrimaryKeys(ref int sequence)
        {
            foreach (var column in Columns.Where(w => w.IsKey))
            {
                sequence++;
                column.SequenceNumber = sequence;
                column.OrderType = ObjectColumnOrderType.Ascending;
            }
        }

        public bool HasValidation()
        {
            return Columns.Any(w => w.HasWheres);
        }

        public string GetSelectColumns()
        {
            // if we are returning a dynamic result we need to only return the column name,
            // the reader will handle the reading correctly.  The only way for a column to be renamed is through returning a dynamic
            return Columns.Where(w => w.IsSelected)
                .Aggregate(string.Empty, (current, column) => current + string.Format("[{0}].[{1}] as [{2}{3}],",
                    column.HasTableAlias
                        ? string.IsNullOrWhiteSpace(column.TableAlias) ? column.TableName : column.TableAlias
                        : column.TableName,
                    column.Name,
                    column.HasColumnAlias ? string.Empty : column.HasTableAlias ? column.TableAlias : column.TableName,
                    column.HasColumnAlias ? column.ColumnAlias : column.Name));
        }

        public string GetSelectColumnsUnAliased()
        {
            return Columns.Where(w => w.IsSelected)
                .Aggregate(string.Empty, (current, column) => current + string.Format("[{0}].[{1}],",
                    column.HasTableAlias
                        ? string.IsNullOrWhiteSpace(column.TableAlias) ? column.TableName : column.TableAlias
                        : column.TableName,
                    column.Name));
        }

        public string GetJoins()
        {
            return Columns.Where(w => w.HasJoins)
                .Aggregate("", (current, objectColumn) => current + objectColumn.GetJoinText());
        }

        public void GetWheres(WhereContainer whereContainer)
        {
            foreach (var column in Columns.Where(w => w.HasWheres))
            {
                column.GetWhereContainer(whereContainer);
            }
        }

        public IEnumerable<ObjectColumn> GetOrderByColumns()
        {
            return Columns.Where(w => w.HasOrderSequence);
        }

        public void UnSelectAll()
        {
            foreach (var column in Columns)
            {
                column.IsSelected = false;
            }
        }

        public void SelectAll()
        {
            foreach (var column in Columns)
            {
                column.IsSelected = true;
            }
        }
        #endregion

        #region IEquatable
        public bool Equals(ObjectTable other)
        {
            if (ReferenceEquals(other, null)) return false;

            if (ReferenceEquals(this, other)) return true;

            return Alias == other.Alias;
        }

        public override int GetHashCode()
        {
            return Alias.GetHashCode();
        }
        #endregion
    }
}
