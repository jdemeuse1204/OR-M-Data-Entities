using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Query;
using OR_M_Data_Entities.Expressions.Resolution.Base;

namespace OR_M_Data_Entities.Expressions.Resolution.Containers
{
    public class JoinResolutionContainer : IResolutionContainer
    {
        public JoinResolutionContainer()
        {
            _joins = new List<JoinGroup>();
        }

        public bool HasItems
        {
            get
            {
                return _joins != null && _joins.Count > 0;
            }
        }

        private readonly List<JoinGroup> _joins;

        public IEnumerable<JoinGroup> Joins { get { return _joins; } }


        public void AddJoin(JoinGroup join)
        {
            _joins.Add(join);
        }


        public string Resolve()
        {
            return _joins.Aggregate("",
                (current, @join) =>
                    current +
                    string.Format(" {0} JOIN {1} On [{2}].[{3}] = [{4}].[{5}] ",
                        _getJoinName(@join.JoinType),
                        @join.Left.HasAlias
                            ? string.Format("[{0}] AS [{1}]", @join.Left.TableName, @join.Left.TableAlias)
                            : string.Format("[{0}]", @join.Left.TableName),
                        @join.Left.HasAlias ? @join.Left.TableAlias : @join.Left.TableName,
                        @join.Left.ColumnName,
                        @join.Right.TableName,
                        @join.Right.ColumnName));
        }

        public IEnumerable<TableInfo> GetChangeTableContainers()
        {
            return _joins.Where(w => w.Left.Change != null).Select(w => w.Left.Change);
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

    public class LeftJoinNode : JoinNode
    {
    }

    public class RightJoinNode : JoinNode
    {
    }

    public abstract class JoinNode
    {
        public string TableName { get; set; }

        public string TableAlias { get; set; }

        public string ColumnName { get; set; }

        public TableInfo Change { get; set; }

        public bool HasAlias
        {
            get { return !string.IsNullOrWhiteSpace(TableAlias) && TableAlias != TableName; }
        }
    }

    public class JoinGroup
    {
        public JoinType JoinType { get; set; }

        public LeftJoinNode Left { get; set; }

        public RightJoinNode Right { get; set; }
    }
}
