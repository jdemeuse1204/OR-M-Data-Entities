/*
 * OR-M Data Entities v2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */
using System.Data;

namespace OR_M_Data_Entities.Commands.Transform
{
    public class TransformContainer
    {
        public SqlDbType? CastType { get; set; }

        public SqlDbType? ConvertType { get; set; }

        public int? ConvertStyle { get; set; }

        public bool IsTransforming { get { return CastType != null || ConvertType != null; } }

        public bool IsCasting { get { return CastType != null; } }

        public bool IsConverting { get { return ConvertType != null; } }

        public string TransformString { get
        {
            return IsCasting
                ? "CAST({0} AS " + CastType + ")"
                : IsConverting
                    ? "CONVERT(" + ConvertType + ",{0}" +
                      (ConvertStyle == null ? ")" : "," + ConvertStyle + ")")
                    : string.Empty;
        } }
    }
}
