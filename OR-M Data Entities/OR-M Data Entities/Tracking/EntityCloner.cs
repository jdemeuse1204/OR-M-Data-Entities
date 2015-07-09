﻿/*
 * OR-M Data Entities v2.1
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Linq;
using OR_M_Data_Entities.Schema;

namespace OR_M_Data_Entities.Tracking
{
    public class EntityCloner
    {
        public static object Clone(object table)
        {
            var instance = Activator.CreateInstance(table.GetType());

            foreach (var item in table.GetType().GetProperties().Where(w => w.IsColumn()))
            {
                var value = item.GetValue(table);

                instance.GetType().GetProperty(item.Name).SetValue(instance, value);
            }

            return instance;
        }
    }
}
