using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Enumeration;

namespace OR_M_Data_Entities.Expressions.Resolution.Containers
{
    public class JoinResolutionContainer
    {
        public JoinResolutionContainer()
        {
            _joins = new List<JoinNode>();
        }

        private readonly List<JoinNode> _joins;
        public IEnumerable<JoinNode> Joins { get { return _joins; } }


        public void AddJoin(JoinNode join)
        {
            _joins.Add(join);
        }

        public int NextJoinId()
        {
            return _joins.Count == 0 ? 1 : (_joins.Select(w => w.JoinId).Max() + 1);
        }
    }

    public class JoinNode
    {
        public string TableName { get; set; }

        public string ColumnName { get; set; }

        public int JoinId { get; set; }

        public JoinType JoinType { get; set; }
    }
}
