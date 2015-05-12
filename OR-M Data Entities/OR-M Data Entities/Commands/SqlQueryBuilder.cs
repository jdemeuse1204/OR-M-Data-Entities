/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using OR_M_Data_Entities.Commands.Secure.StatementParts;
using OR_M_Data_Entities.Commands.Support;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;

namespace OR_M_Data_Entities.Commands
{
    public sealed class SqlQueryBuilder : SqlValidation, ISqlBuilder
    {
        #region Properties
        private Dictionary<string, QueryBuilderJoin> _joins { get; set; } 
        private string _select { get; set; }
        private string _columns { get; set; }
        private string _straightSelect { get; set; }
        private Type _tableType { get; set; }
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
            _tableType = null;
        }
        #endregion

        #region Methods
        public SqlCommand Build(SqlConnection connection, out DataQueryType dataQueryType)
        {
            dataQueryType = _tableType != null
                ? DatabaseSchemata.HasForeignKeys(_tableType) ? DataQueryType.ForeignKeys : DataQueryType.NoForeignKeys
                : DataQueryType.NoForeignKeys;

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

            _tableType = null;
        }

        public void SelectTop(int rows, string table, params Field[] fields)
        {
            _select = string.Format(" SELECT TOP {0} ", rows);

            foreach (var field in fields)
            {
                var alias = string.IsNullOrWhiteSpace(field.Alias) ? "" : string.Format(" AS {0}", field.Alias);
                _columns += string.Format("[{0}].[{1}]{2},", table, field.ColumnName, alias);
            }

            _tableType = null;
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

    }

    #region helpers
    class QueryBuilderJoin
    {
        public string ParentTableName { get; set; }

        public string ParentColumnName { get; set; }

        public string JoinTableName { get; set; }

        public string JoinColumnName { get; set; }

        public JoinType Type { get; set; }

        public string JoinTableAlias { get; set; }

        public string GetJoinText()
        {
            if (!string.IsNullOrWhiteSpace(JoinTableAlias))
            {
                const string joinText = " {0} JOIN {1} as {2} On [{4}].[{5}] = [{2}].[{3}] ";

                switch (Type)
                {
                    case JoinType.Inner:
                        return string.Format(joinText,
                            "INNER",
                            JoinTableName,
                            JoinTableAlias,
                            JoinColumnName,
                            ParentTableName,
                            ParentColumnName);
                    case JoinType.Left:
                        return string.Format(joinText,
                            "LEFT",
                            JoinTableName,
                            JoinTableAlias,
                            JoinColumnName,
                            ParentTableName,
                            ParentColumnName);
                    default:
                        return string.Empty;
                }
            }

            switch (Type)
            {
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
    #endregion
}
