using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Expressions.Query;

namespace OR_M_Data_Entities.Expressions.Collections
{
    public class TableTypeCollection : ReadOnlyTableTypeCollection
    {
        public string Add(PartialTableType partialTable)
        {
            var aka = string.Format("AkA{0}", Internal.Count);

            Internal.Add(new TableType(partialTable.Type, aka, partialTable.PropertyName));

            return aka;
        }

        public void AddRange(IEnumerable<PartialTableType> range)
        {
            Internal.AddRange(
                range.Select(
                    w =>
                        new TableType(w.Type, string.Format("AkA{0}", Internal.Count),
                            w.PropertyName)));
        }

        public TableType this[int i]
        {
            get { return Internal[i]; }
        }
    }
}
