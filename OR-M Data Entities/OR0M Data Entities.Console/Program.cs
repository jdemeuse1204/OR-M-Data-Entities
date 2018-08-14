﻿using Benchmark;
using NickOfTime.ServiceModels.DataTransferObjects.ORM;
using OR_M_Data_Entities.Lite;
using OR_M_Data_Entities.Lite.Mapping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace OR0M_Data_Entities.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var list = new List<int> { 1, 2, 4 };
            var duration = Profiler.Profile("", 1, () =>
            {
                var context = new DbSqlLiteContext("");

                var result = context.From<ReadOnlyJob>().FirstOrDefault(w => w.JobId == 1 && w.IntakeTypeId == 1 || w.JobId == 2 && list.Contains(w.JobId));

            });

            if (duration != null)
            {

            }
        }
    }
}
