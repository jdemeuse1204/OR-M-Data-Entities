using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using OR_M_Data_Entities;
using OR_M_Data_Entities.Commands.Transform;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Scripts;
using OR_M_Data_Entities.Tests.Tables;

namespace OR0M_Data_Entities.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            // after save, need to update the _tableOnLoad to match
            var context = new DbSqlContext("sqlExpress");

            var reader = context.ExecuteStoredSql<StoredProcedures, int>(w => w.GetUsername).FirstOrDefault();

            if (reader != 0)
            {
                
            }

            var appointment = new Appointment();
            appointment.ContactID = 10;
            appointment.Description = "NEW";
            appointment.IsScheduled = false;

            //context.Delete(appointment);

            var policy = context.Find<Contact>(10);

            // Change first or default/first?  do select top 1 (primary key(s)) to get the keys, then to select for first /firstdefault?


            context.SaveChanges(policy);

            var entityState = policy.GetState();

            policy.FirstName = "CHANGED!";

            entityState = policy.GetState();

            if (entityState == EntityState.Modified)
            {
                context.SaveChanges(policy);

            }


            entityState = policy.GetState();
        }

        public class StoredProcedures : StoredSql
        {
            public string GetUsername = "Select Top 1 1 From Parent";

            public string ResetAllOrdersToNewOrders { get; set; }
        }
    }
}
