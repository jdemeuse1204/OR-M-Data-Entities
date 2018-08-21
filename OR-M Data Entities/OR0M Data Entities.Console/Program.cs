using Benchmark;
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
                var context = new DbSqlLiteContext("Application Name=NickOfTimeMoving;Data Source=198.71.225.145;Initial Catalog=NickOfTimeMoving;Persist Security Info=true;User ID=jdemeuse;Password=j*ehE177;Pooling=true;Max Pool Size=50;");
                //w.JobId == 1 && w.IntakeTypeId == 1 || w.JobId == 2 && 
                var result = context.From<ReadOnlyJob>().FirstOrDefault(w => w.JobId == 100);

            });

            if (duration != null)
            {

            }
        }
    }
}
