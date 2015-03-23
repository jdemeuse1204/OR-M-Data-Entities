/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using OR_M_Data_Entities.Commands.Support;
using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities.Commands
{
    public sealed class SqlQueryBuilder : SqlValidation, ISqlBuilder
    {
        #region Properties
        private string _join { get; set; }
        private string _select { get; set; }
        private string _columns { get; set; }
        private string _straightSelect { get; set; }
        private Dictionary<string, object> _parameters { get; set; }
        #endregion

        #region Constructor
        public SqlQueryBuilder()
        {
            _join = string.Empty;
            _select = string.Empty;
            _columns = string.Empty;
            _straightSelect = string.Empty;
            _parameters = new Dictionary<string, object>();
        }
        #endregion

        #region Methods
        public SqlCommand Build(SqlConnection connection)
        {
            if (string.IsNullOrWhiteSpace(_select) && string.IsNullOrWhiteSpace(_straightSelect))
            {
                throw new QueryNotValidException("SELECT statement missing");
            }

            if (string.IsNullOrWhiteSpace(TableName) && string.IsNullOrWhiteSpace(_straightSelect))
            {
                throw new QueryNotValidException("Table statement missing");
            }

            var sql = string.IsNullOrWhiteSpace(_straightSelect) ?
                _select + _columns.TrimEnd(',') + string.Format(" FROM {0} ", TableName) + _join + GetValidation() :
                _straightSelect;

            var cmd = new SqlCommand(sql, connection);

            InsertParameters(cmd);

            return cmd;
        }

        public void Select(string table, params Field[] fields)
        {
            _select = " SELECT ";

            foreach (var field in fields)
            {
                var alias = string.IsNullOrWhiteSpace(field.Alias) ? "" : string.Format(" AS {0}", field.Alias);
                _columns += string.Format("[{0}].[{1}]{2},", table, field.ColumnName, alias);
            }
        }

        public void SelectAll(Type tableType)
        {
            _createSelectStatement(tableType, " SELECT ");
        }

        public void SelectAll<T>() where T : class
        {
            SelectAll(typeof(T));
        }

        public void SelectTopOneAll(Type tableType)
        {
            _createSelectStatement(tableType, " SELECT TOP 1 ");
        }

        public void SelectTopOneAll<T>() where T : class
        {
            _createSelectStatement(typeof(T), " SELECT TOP 1 ");
        }

        public void SelectTop(int rows, string table, params Field[] fields)
        {
            _select = string.Format(" SELECT TOP {0} ", rows);

            foreach (var field in fields)
            {
                var alias = string.IsNullOrWhiteSpace(field.Alias) ? "" : string.Format(" AS {0}", field.Alias);
                _columns += string.Format("[{0}].[{1}]{2},", table, field.ColumnName, alias);
            }
        }

        public void Select(string sql)
        {
            _straightSelect = sql;
        }

        public void AddJoin(JoinType type, string parentTable, string parentField, string childTable, string childField)
        {
            switch (type)
            {
                case JoinType.Equi:
                    _join += string.Format(",{0}", childTable);
                    AddWhere(parentTable, parentField, childTable, childField);
                    break;
                case JoinType.Inner:
                    _join += string.Format(" INNER JOIN [{0}] On [{1}].[{2}] = [{3}].[{4}] ",
                        parentTable,
                        parentTable,
                        parentField,
                        childTable,
                        childField);
                    break;
                case JoinType.Left:
                    _join += string.Format(" LEFT JOIN [{0}] On [{1}].[{2}] = [{3}].[{4}] ",
                        parentTable,
                        parentTable,
                        parentField,
                        childTable,
                        childField);
                    break;
            }
        }
        #endregion

        #region Helpers
        private void _createSelectStatement(Type tableType, string beginningSelectStatement)
        {
            _select = beginningSelectStatement;

            var selectAllTableNameAndFields = DatabaseSchemata.GetSelectAllFieldsAndTableName(tableType);
            var tableName = selectAllTableNameAndFields.Key;
            var fields = selectAllTableNameAndFields.Value;

            foreach (var field in fields)
            {
                _select += string.Format("[{0}].[{1}],", tableName, field);
            }

            _select = _select.TrimEnd(',');
        }
        #endregion
    }
}
