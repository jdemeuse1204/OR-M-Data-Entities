﻿/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Expressions.Resolution.Join;
using OR_M_Data_Entities.Extensions;

namespace OR_M_Data_Entities.Expressions.Resolution.Containers
{
    public class JoinResolutionContainer : ResolutionContainerBase, IResolutionContainer
    {
        public IEnumerable<JoinTablePair> Joins { get { return _joins; } }

        private List<JoinTablePair> _joins;

        public JoinResolutionContainer(IEnumerable<JoinTablePair> joins, Guid expressionQueryId)
            : base(expressionQueryId)
        {
            _joins = new List<JoinTablePair>();
            _joins.AddRange(joins);
        }

        public bool HasItems
        {
            get
            {
                return _joins != null && _joins.Count > 0;
            }
        }

        public void Combine(IResolutionContainer container)
        {
            _joins = container.GetType()
                .GetField("_joins", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(container) as List<JoinTablePair>;
        }

        public void Add(JoinTablePair join)
        {
            _joins.Add(join);
        }

        public void ClearJoins()
        {
            _joins.Clear();
        }

        public string Resolve(string viewId = null)
        {
            return _joins.Aggregate("",
                (current, @join) => current + string.Format(" {0} JOIN {1} On [{2}].[{3}] = [{4}].[{5}] ",

                    @join.HeirarchyContainsList ? "LEFT" : "INNER",

                    string.Format("{0} As [{1}]",
                        @join.ChildTable.TableInfo.IsUsingLinkedServer
                            ? _getLinkedServerJoinSubQuery(@join)
                            : @join.ChildTable.TableInfo.ToString(),
                        @join.ChildTable.Alias),

                    @join.ParentTable.Alias,

                    @join.ParentTable.GetForeignKeyDatabaseColumnName(),

                    @join.ChildTable.Alias,

                    @join.ChildTable.GetForeignKeyDatabaseColumnName()));
        }

        private string _getLinkedServerJoinSubQuery(JoinTablePair pair)
        {
            var query = new SqlLinkedServerSelectBuilder();
            query.Table(pair.ChildTable.Type);
            query.SetValidation(pair);
            query.SelectAll(pair);
            return query.GetSql();
        }

        private string _getJoinName(JoinType joinType)
        {
            switch (joinType)
            {
                case JoinType.ForeignKeyLeft:
                case JoinType.Left:
                case JoinType.PseudoKeyLeft:
                    return "LEFT";
                case JoinType.ForeignKeyInner:
                case JoinType.Inner:
                case JoinType.PseudoKeyInner:
                    return "INNER";
                default:
                    return "";
            }
        }

        #region helpers 

        private class SqlLinkedServerSelectBuilder : SqlQueryBuilder
        {
            private string _validation { get; set; }

            public override string GetSql()
            {
                if (string.IsNullOrWhiteSpace(TableName) && string.IsNullOrWhiteSpace(StraightSelect))
                {
                    throw new QueryNotValidException("Table statement missing");
                }

                return string.Format("(SELECT {0} FROM [{1}] WHERE {2})", 
                    Columns.TrimEnd(','),
                    TableName.TrimStart('[').TrimEnd(']'),
                    _validation);
            }

            public void SelectAll(JoinTablePair pair)
            {
                foreach (var column in pair.ChildTable.Type.GetProperties().Where(w => w.IsColumn()))
                {
                    Columns = string.Concat(Columns,
                        string.Format("[{0}],", column.GetColumnName()));
                }
            }

            public void SetValidation(JoinTablePair pair)
            {
                _validation = string.Format("[{0}] IN (SELECT DISTINCT [{1}] FROM {2})",
                    pair.ChildTable.GetForeignKeyDatabaseColumnName(),
                    pair.ParentTable.GetForeignKeyDatabaseColumnName(), 
                    pair.ParentTable.TableInfo);
            }
        }
        #endregion
    }
}
