/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Data;

namespace OR_M_Data_Entities.Commands.Transform
{
    public class DbFunctions
    {
        #region Cast
        public static object Cast(object entity, SqlDbType targetTransformType)
        {
            return entity;
        }

        public static DateTime Cast(DateTime entity, SqlDbType targetTransformType)
        {
            return entity;
        }

        public static int Cast(int entity, SqlDbType targetTransformType)
        {
            return entity;
        }

        public static string Cast(string entity, SqlDbType targetTransformType)
        {
            return entity;
        }
        #endregion

        #region Convert
        public static object Convert(SqlDbType targetTransformType, object entity, int? style = null)
        {
            return entity;
        }
        #endregion
    }
}
