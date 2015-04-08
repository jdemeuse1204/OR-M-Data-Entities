/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */
namespace OR_M_Data_Entities.Expressions.Support
{
    public enum SqlExpressionType
    {
        ForeignKeySelect,
        ForeignKeySelectJoin,
        ForeignKeySelectWhere,
        ForeignKeySelectWhereJoin,
        Select,
        SelectJoin,
        SelectWhere,
        SelectWhereJoin
    }
}
