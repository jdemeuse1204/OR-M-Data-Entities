using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities.Commands.StatementParts
{
    public class SqlSelect : IEnumerable<SqlColumn>
    {
        private readonly List<SqlColumn> _list;
        public IEnumerable<SqlColumn> Columns { get { return _list; } }

        public SqlTable Table { get; set; }

        public SqlSelect(Type tableType)
        {
            Table = new SqlTable(tableType);
            _list = new List<SqlColumn>();
        }

        public void SelectAll()
        {
            foreach (var sqlColumn in DatabaseSchemata.GetTableFields(Table.TableType))
            {
                _list.Add(new SqlColumn(sqlColumn));
            }
        }

        public string GetSelectText()
        {
            var result = _list.Aggregate(string.Empty, (current, column) => current + string.Format("{0},", column.GetColumnText(Table.TableName)));

            return result.TrimEnd(',');
        }

        public void Add(SqlColumn column)
        {
            _list.Add(column);
        }

        public IEnumerator<SqlColumn> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
