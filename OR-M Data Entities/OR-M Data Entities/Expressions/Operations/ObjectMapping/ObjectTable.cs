using System;
using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions.Operations.Payloads.Base;

namespace OR_M_Data_Entities.Expressions.Operations.ObjectMapping
{
    public class ObjectTable : IEquatable<ObjectTable>
    {
        #region Constructor
        public ObjectTable(Type type, string alias, string tableName, bool isBaseTable = false, bool selectAllColumns = true)
        {
            var i = -1;
            Alias = alias;
            TableName = tableName;
            Columns = DatabaseSchemata.GetTableFields(type).Select(w => new ObjectColumn(w, ++i, tableName, alias, selectAllColumns)).ToList();
            Type = type;
            IsBaseTable = isBaseTable;
        }
        #endregion

        #region Properties
        public Type Type { get; set; }

        public string Alias { get; set; }

        public string TableName { get; set; }

        public bool IsBaseTable { get; private set; }

        public List<ObjectColumn> Columns { get; set; }

        public bool HasAlias { get { return !TableName.Equals(Alias); } }

        public bool HasValidation()
        {
            return Columns.Any(w => w.HasWheres);
        }

        public string GetSelectColumns()
        {
            return Columns.Where(w => w.IsSelected).Aggregate(string.Empty, (current, column) => current + string.Format("[{0}].[{1}] as [{0}{1}],",
                column.HasAlias ? column.TableAlias : column.TableName,
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
