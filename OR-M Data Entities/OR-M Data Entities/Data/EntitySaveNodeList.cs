using System.Collections;
using System.Collections.Generic;
using OR_M_Data_Entities.Data.Execution;

namespace OR_M_Data_Entities.Data
{
    public class EntitySaveNodeList : IEnumerable<ForeignKeySaveNode>
    {
        public int Count
        {
            get { return _internal.Count; }
        }

        public ForeignKeySaveNode this[int i]
        {
            get { return _internal[i] as ForeignKeySaveNode; }
        }

        private readonly List<object> _internal;

        public EntitySaveNodeList() 
        {
            _internal = new List<object>();
        }

        public EntitySaveNodeList(ForeignKeySaveNode node)
            : this()
        {
            _internal.Add(node);
        }
        
        public int IndexOf(object entity)
        {
            return _internal.IndexOf(entity);
        }

        public void Insert(int index, ForeignKeySaveNode node)
        {
            _internal.Insert(index, node);
        }

        public void Add(ForeignKeySaveNode node)
        {
            _internal.Add(node);
        }

        public void Reverse()
        {
            _internal.Reverse();
        }

        public IEnumerator<ForeignKeySaveNode> GetEnumerator()
        {
            return ((dynamic)_internal).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
