/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2015 James Demeuse
 */

using System.Data;

namespace OR_M_Data_Entities.Commands.Transform
{
    public class DbTransform
    {
        #region Cast
        public static object Cast(object entity, SqlDbType targetTransformType)
        {
            return entity;
        }
        #endregion

        #region Convert
        public static object Convert(SqlDbType targetTransformType, object entity, int? style)
        {
            return entity;
        }
        #endregion
    }
}
