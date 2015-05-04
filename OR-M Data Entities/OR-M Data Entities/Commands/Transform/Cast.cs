/*
 * OR-M Data Entities v1.1.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */
using System;
/*
 * OR-M Data Entities v1.0.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */
using System.Data;

namespace OR_M_Data_Entities.Commands.Transform
{
    public class Cast
    {
        public static object As(object entity, SqlDbType targetTransformType)
        {
			return entity;
        }

		public static DateTime As(DateTime entity, SqlDbType targetTransformType)
		{
			return entity;
		}

		public static int As(int entity, SqlDbType targetTransformType)
		{
			return entity;
		}

		public static string As(string entity, SqlDbType targetTransformType)
		{
			return entity;
		}
    }
}
