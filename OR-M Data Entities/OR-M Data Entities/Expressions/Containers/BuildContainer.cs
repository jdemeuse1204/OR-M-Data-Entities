/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System.Collections.Generic;

namespace OR_M_Data_Entities.Expressions.Containers
{
    public class BuildContainer
    {
        public BuildContainer()
        {
            Sql = string.Empty;
            Parameters = new Dictionary<string, object>();
        }

        public string Sql { get; set; }

        public Dictionary<string,object> Parameters { get; set; } 
    }
}
