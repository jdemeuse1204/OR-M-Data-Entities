﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace OR_M_Data_Entities.Data.PayloadOperations.ObjectMapping
{
    public class ObjectTable : IEquatable<ObjectTable>
    {
        #region Constructor
        public ObjectTable(Type type, string alias, string tableName)
        {
            Alias = alias;
            TableName = tableName;
            Columns = DatabaseSchemata.GetTableFields(type).Select(w => new ObjectColumn(w, tableName, alias)).ToList();
            Type = type;
        }
        #endregion

        #region Properties
        public Type Type { get; set; }

        public string Alias { get; set; }

        public string TableName { get; set; }

        public List<ObjectColumn> Columns { get; set; }

        public bool HasAlias { get { return !TableName.Equals(Alias); } }

        public string GetSelectColumns()
        {
            return Columns.Aggregate(string.Empty, (current, column) => current + string.Format("[{0}].[{1}],", column.TableAlias, column.Name));
        }


        public string GetJoins()
        {
            return Columns.Where(w => w.HasJoins)
                .Aggregate("", (current, objectColumn) => current + objectColumn.GetJoinText());
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
