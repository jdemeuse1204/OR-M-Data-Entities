/*
 * OR-M Data Entities v1.0.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */
using System.Data;

namespace OR_M_Data_Entities.Commands.Transform
{
    public class Conversion
    {
		public static object To(SqlDbType targetTransformType, object entity, int style)
        {
            return entity;
        }
    }
}
