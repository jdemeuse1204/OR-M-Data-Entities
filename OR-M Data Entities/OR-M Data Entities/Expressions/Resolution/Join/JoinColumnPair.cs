using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Query.Columns;

namespace OR_M_Data_Entities.Expressions.Resolution.Join
{
    public class JoinColumnPair
    {
        public JoinType JoinType { get; set; }

        public PartialColumn ChildColumn { get; set; }

        public PartialColumn ParentColumn { get; set; } 
    }
}
