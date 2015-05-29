using System.Collections.Generic;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Query;
using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Expressions.Resolution.Join;

namespace OR_M_Data_Entities.Expressions.Resolution.Containers
{
    public class JoinResolutionContainer : IResolutionContainer
    {
        public IEnumerable<JoinPair> Joins { get { return _joins; } }

        private readonly List<JoinPair> _joins;

        public JoinResolutionContainer(IEnumerable<JoinPair> joins)
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

        public void Add(JoinPair join)
        {
            _joins.Add(join);
        }

        public string Resolve()
        {
            return "";
            //_joins.Aggregate("",
            //    (current, @join) =>
            //        current +
            //        string.Format(" {0} JOIN {1} On [{2}].[{3}] = [{4}].[{5}] ",
            //            _getJoinName(@join.JoinType),
            //            @join.ParentNode.HasAlias
            //                ? string.Format("[{0}] AS [{1}]", @join.ParentNode.TableName, @join.ParentNode.TableAlias)
            //                : string.Format("[{0}]", @join.ParentNode.TableName),
            //            @join.ParentNode.HasAlias ? @join.ParentNode.TableAlias : @join.ParentNode.TableName,
            //            @join.ParentNode.ColumnName,
            //            @join.ChildNode.TableName,
            //            @join.ChildNode.ColumnName));
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
