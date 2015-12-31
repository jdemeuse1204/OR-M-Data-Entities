/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */
using System.Collections.Generic;
using OR_M_Data_Entities.Expressions.Query.Tables;

namespace OR_M_Data_Entities.Expressions.Collections
{
    public class TableCollection : ReadOnlyTableCollection
    {
        public string Add(ForeignKeyTable foreignKeyTable)
        {
            foreignKeyTable.Alias = string.Format("AkA{0}", Internal.Count);

            Internal.Add(foreignKeyTable);

            return foreignKeyTable.Alias;
        }

        public void Insert(int index, ForeignKeyTable foreignKeyTable)
        {
            var aka = string.Format("AkA{0}", Internal.Count);

            foreignKeyTable.Alias = aka;

            Internal.Insert(index, foreignKeyTable);
        }

        public void AddRange(IEnumerable<ForeignKeyTable> range)
        {
            foreach (var foreignKeyTable in range)
            {
                Internal.Add(new ForeignKeyTable(foreignKeyTable.ExpressionQueryId, foreignKeyTable.Type,
                    foreignKeyTable.ForeignKeyPropertyName,
                    string.Format("AkA{0}", Internal.Count)));
            }
        }

        public ForeignKeyTable this[int i]
        {
            get { return Internal[i]; }
        }
    }
}
