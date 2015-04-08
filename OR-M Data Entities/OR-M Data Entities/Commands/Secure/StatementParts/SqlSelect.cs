/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities.Commands.Secure.StatementParts
{
    public sealed class SqlSelect : IEnumerable<SqlColumn>
    {
        #region Properties and Fields
        private readonly List<SqlColumn> _list;
        public IEnumerable<SqlColumn> Columns { get { return _list; } }

        public SqlTable Table { get; set; }
        #endregion

        #region Constructor
        public SqlSelect(Type tableType)
        {
            Table = new SqlTable(tableType);
            _list = new List<SqlColumn>();
        }
        #endregion

        #region Methods
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
        #endregion
    }
}
