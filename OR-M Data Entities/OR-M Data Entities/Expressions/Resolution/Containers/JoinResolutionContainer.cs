using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Enumeration;
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
                    string.Format(" {0} [{1}] On [{1}].[{2}] = [{3}].[{4}] ", @join.JoinType, @join.Left.TableName,
                        @join.Left.ColumnName, @join.Right.TableName, @join.Right.ColumnName));
        }
    }

    public class JoinNode
    {
        // alias is not needed, foreign keys will not use this to join

        public string TableName { get; set; }

        public string ColumnName { get; set; }
    }

    public class JoinGroup
    {
        public JoinType JoinType { get; set; }

        public JoinNode Left { get; set; }

        public JoinNode Right { get; set; }
    }
}
