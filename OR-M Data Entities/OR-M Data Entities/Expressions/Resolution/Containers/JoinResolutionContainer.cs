using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Query;
using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Expressions.Resolution.Join;

namespace OR_M_Data_Entities.Expressions.Resolution.Containers
{
    public class JoinResolutionContainer : ResolutionContainerBase, IResolutionContainer
    {
        public IEnumerable<JoinPair> Joins { get { return _joins; } }

        private List<JoinPair> _joins;

        public JoinResolutionContainer(IEnumerable<JoinPair> joins, Guid expressionQueryId)
            : base(expressionQueryId)
        {
            _joins = new List<JoinPair>();
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
                .GetValue(container) as List<JoinPair>;
        }

        public void Add(JoinPair join)
        {
            _joins.Add(join);
        }

        public void ClearJoins()
        {
            _joins.Clear();
        }

        public string Resolve()
        {
            return _joins.Aggregate("",
                (current, @join) => current + string.Format(" {0} JOIN {1} On [{2}].[{3}] = [{4}].[{5}] ",
                    @join.HeirarchyContainsList ? "LEFT" : "INNER",
                    string.Format("[{0}] As [{1}]", @join.ChildTableName, @join.ComputedChildAlias),
                    @join.ComputedParentAlias, @join.ParentPropertyName, @join.ComputedChildAlias,
                    @join.ChildPropertyName));
        }

        public IEnumerable<TableInfo> GetChangeTableContainers()
        {
            return null;// _joins.Where(w => w.ParentNode.TableInfo != null).Select(w => w.ParentNode.TableInfo);
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
    }
}
