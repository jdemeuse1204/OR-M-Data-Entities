using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OR_M_Data_Entities.Expressions.Resolution.Containers
{
    public class SelectResolutionContainer
    {
        private readonly List<SelectNode> _columns;
        public IEnumerable<SelectNode> Columns { get { return _columns; } }

        public SelectResolutionContainer()
        {
            _columns = new List<SelectNode>();
        }

        public void AddColumn(SelectNode node)
        {
            if (node.Ordinal != 0) throw new Exception("Ordinal set automatically, must be 0");

            var nextOrdinal = _columns.Count == 0 ? 0 : (_columns.Select(w => w.Ordinal).Max() + 1);

            node.Ordinal = nextOrdinal;

            _columns.Add(node);
        }
    }

    public class SelectNode
    {
        public string TableName { get; set; }

        public string ColumnName { get; set; }

        public int Ordinal { get; set; }

        public MemberInfo MappedProperty { get; set; }
    }
}
