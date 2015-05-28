/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using OR_M_Data_Entities.Expressions.Containers;
using OR_M_Data_Entities.Expressions.ObjectMapping.Base;

namespace OR_M_Data_Entities.Expressions.Resolution
{
    public abstract class Builder
    {
        public ObjectMap Map { get; protected set; }
        protected abstract BuildContainer Build(string viewId);
    }
}
