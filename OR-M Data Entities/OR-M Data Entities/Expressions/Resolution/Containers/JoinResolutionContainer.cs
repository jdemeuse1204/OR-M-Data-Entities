/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Expressions.Resolution.Join;

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
            //var query = new SqlLinkedServerSelectBuilder();
            //query.Table(pair.ChildTable.Type);
            //query.SetValidation(pair);
            //query.SelectAll(pair);
            //return query.GetSql();

            return "";
        }
    }
}
