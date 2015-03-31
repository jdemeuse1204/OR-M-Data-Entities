/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using OR_M_Data_Entities.Commands.Support;
using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities.Commands
{
    public sealed class SqlQueryBuilder : SqlValidation, ISqlBuilder
    {
        #region Properties
        private Dictionary<string, QueryBuilderJoin> _joins { get; set; } 
        private string _select { get; set; }
        private string _columns { get; set; }
        private string _straightSelect { get; set; }
        private Dictionary<string, object> _parameters { get; set; }
        #endregion

        #region Constructor
        public SqlQueryBuilder()
        {
            _select = string.Empty;
            _columns = string.Empty;
            _straightSelect = string.Empty;
            _parameters = new Dictionary<string, object>();
            _joins = new Dictionary<string, QueryBuilderJoin>();
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

            var joinSql = _joins.Aggregate(string.Empty, (current, @join) => current + @join.Value.GetJoinText());

            var sql = string.IsNullOrWhiteSpace(_straightSelect) ?
                _select + _columns.TrimEnd(',') + string.Format(" FROM {0} ", TableName) + joinSql + GetValidation() :
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
            var joinKey = parentTable + childField;

            if (_joins.ContainsKey(joinKey)) return;

            switch (type)
            {
                case JoinType.Equi:

                    _joins.Add(joinKey, new QueryBuilderJoin
                    {
                        JoinColumnName = childField,
                        JoinTableName = childTable,
                        ParentColumnName = parentField,
                        ParentTableName = parentTable,
                        Type = type
                    });

                    break;
                case JoinType.Inner:

                    _joins.Add(joinKey, new QueryBuilderJoin
                    {
                        JoinColumnName = childField,
                        JoinTableName = childTable,
                        ParentColumnName = parentField,
                        ParentTableName = parentTable,
                        Type = type
                    });

                    break;
                case JoinType.Left:

                    _joins.Add(joinKey, new QueryBuilderJoin
                    {
                        JoinColumnName = childField,
                        JoinTableName = childTable,
                        ParentColumnName = parentField,
                        ParentTableName = parentTable,
                        Type = type
                    });

                    break;
            }
        }
        #endregion

        #region Helpers
        private void _createSelectStatement(Type tableType, string beginningSelectStatement)
        {
            _select = string.Empty;
            _select = beginningSelectStatement;

            if (DatabaseSchemata.HasForeignKeys(tableType))
            {
                var allJoinsFromForeignKeys = DatabaseSchemata.GetForeignKeyJoinsRecursive(tableType);

                for (var i = 0; i < allJoinsFromForeignKeys.Keys.Count(); i++)
                {
                    var composite = allJoinsFromForeignKeys.GetKey(i);
                    var key = DatabaseSchemata.GetTableName(composite.Key) +
                              DatabaseSchemata.GetTableName(composite.Value);
                    var join = allJoinsFromForeignKeys.GetJoin(i);

                    _joins.Add(key, new QueryBuilderJoin
                    {
                        ParentTableName = DatabaseSchemata.GetTableName(join.ParentEntity.Table),
                        ParentColumnName = DatabaseSchemata.GetColumnName(join.ParentEntity.Column),

                        JoinTableName = DatabaseSchemata.GetTableName(join.JoinEntity.Table),
                        JoinColumnName = DatabaseSchemata.GetColumnName(join.JoinEntity.Column),

                        Type = join.Type
                    });
                }

                foreach (var table in allJoinsFromForeignKeys.SelectedTypes)
                {
                    var tableNameAndColumnNames = DatabaseSchemata.GetSelectAllFieldsAndTableName(table);
                    var tName = tableNameAndColumnNames.Key;
                    var columns = tableNameAndColumnNames.Value;

                    _select += columns.Aggregate("", (current, column) => current + string.Format("[{0}].[{1}],", tName, column));
                }
                
                _select = _select.TrimEnd(',');

                return;
            }

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

    class QueryBuilderJoin
    {
        public string ParentTableName { get; set; }

        public string ParentColumnName { get; set; }

        public string JoinTableName { get; set; }

        public string JoinColumnName { get; set; }

        public JoinType Type { get; set; }

        public string GetJoinText()
        {
            switch (Type)
            {
                case JoinType.Equi:
                    return string.Format("[{0}].[{1}] = [{2}].[{3}]", 
                        ParentTableName, 
                        ParentColumnName, 
                        JoinTableName,
                        JoinColumnName);
                case JoinType.Inner:
                    return string.Format(" INNER JOIN [{0}] On [{0}].[{1}] = [{2}].[{3}] ", 
                        JoinTableName,
                        JoinColumnName, 
                        ParentTableName, 
                        ParentColumnName);
                case JoinType.Left:
                    return string.Format(" LEFT JOIN [{0}] On [{0}].[{1}] = [{2}].[{3}] ", 
                        JoinTableName,
                        JoinColumnName, 
                        ParentTableName, 
                        ParentColumnName);
                default:
                    return string.Empty;
            }
        }
    }
}
