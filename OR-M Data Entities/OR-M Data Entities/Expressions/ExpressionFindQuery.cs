/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions.ObjectMapping.Base;

namespace OR_M_Data_Entities.Expressions
{
    public class ExpressionFindQuery : ExpressionQuery
    {
        public ExpressionFindQuery(ObjectMap map, DataFetching context)
			: base(context)
		{
			Map = map;
		}

        public T Find<T>(object[] pks) 
            where T : class 
        {
            var select = new ExpressionSelectQuery(Map, Context);
            var selectAll = select.SelectAll<T>();

            _addWhere(pks, selectAll.Map);

            return selectAll.First<T>();
        }

        private void _addWhere(object[] pks, ObjectMap map)
        {
            var baseTable = map.Tables.First(w => w.IsBaseTable);
            var index = 0;

            foreach (var key in baseTable.Columns.Where(w => w.IsKey).OrderBy(w => w.Order))
            {
                key.CompareValues.Add(new KeyValuePair<object, ComparisonType>(pks[index], ComparisonType.Equals));

                index++;
            }
        }
    }
}
