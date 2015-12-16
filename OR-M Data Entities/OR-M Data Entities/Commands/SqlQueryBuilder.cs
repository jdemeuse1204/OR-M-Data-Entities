﻿/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using OR_M_Data_Entities.Commands.Secure.StatementParts;
using OR_M_Data_Entities.Commands.Support;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Exceptions;

namespace OR_M_Data_Entities.Commands
{
    [Obsolete("Expression Query should be used instead.  If using please contact me and I will leave these in.")]
    public class SqlQueryBuilder : SqlValidation, ISqlBuilder
    {
        #region Properties
        private Dictionary<string, QueryBuilderJoin> _joins { get; set; } 
        protected string SelectStatement { get; set; }
        protected string Columns { get; set; }
        protected string StraightSelect { get; set; }
        protected Type TableType { get; set; }
        protected Dictionary<string, object> Parameters { get; set; }
        #endregion

        #region Constructor
        public SqlQueryBuilder()
        {
            SelectStatement = string.Empty;
            Columns = string.Empty;
            StraightSelect = string.Empty;
            Parameters = new Dictionary<string, object>();
            _joins = new Dictionary<string, QueryBuilderJoin>();
            TableType = null;
        }
        #endregion

        #region Methods
        public SqlCommand Build(SqlConnection connection)
        {
            var sql = GetSql();

            var cmd = new SqlCommand(sql, connection);

            InsertParameters(cmd);

            return cmd;
        }

        public virtual string GetSql()
        {
            if (string.IsNullOrWhiteSpace(SelectStatement) && string.IsNullOrWhiteSpace(StraightSelect))
            {
                throw new QueryNotValidException("SELECT statement missing");
            }

            if (string.IsNullOrWhiteSpace(TableName) && string.IsNullOrWhiteSpace(StraightSelect))
            {
                throw new QueryNotValidException("Table statement missing");
            }

            var joinSql = _joins.Aggregate(string.Empty, (current, @join) => current + @join.Value.GetJoinText());

            return string.IsNullOrWhiteSpace(StraightSelect) ?
                SelectStatement + Columns.TrimEnd(',') + string.Format(" FROM [{0}] ", TableName.TrimStart('[').TrimEnd(']')) + joinSql + GetValidation() :
                StraightSelect;
        }

        public void Select(string table, params Field[] fields)
        {
            SelectStatement = " SELECT ";

            foreach (var field in fields)
            {
                var alias = string.IsNullOrWhiteSpace(field.Alias) ? "" : string.Format(" AS {0}", field.Alias);
                Columns += string.Format("[{0}].[{1}]{2},", table, field.ColumnName, alias);
            }

            TableType = null;
        }

        public void SelectTop(int rows, string table, params Field[] fields)
        {
            SelectStatement = string.Format(" SELECT TOP {0} ", rows);

            foreach (var field in fields)
            {
                var alias = string.IsNullOrWhiteSpace(field.Alias) ? "" : string.Format(" AS {0}", field.Alias);
                Columns += string.Format("[{0}].[{1}]{2},", table, field.ColumnName, alias);
            }

            TableType = null;
        }

        public void Select(string sql)
        {
            StraightSelect = sql;
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
