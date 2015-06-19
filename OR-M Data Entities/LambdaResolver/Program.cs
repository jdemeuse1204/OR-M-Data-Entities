using System;
using System.Collections.Generic;
using System.Linq;
using ORSigningPro.Common.Data.ORTSigningPro.Tables;
using OR_M_Data_Entities;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Tests.Tables;

namespace LambdaResolver
{
    class Program
    {
        private static void Main(string[] args)
        {
            var x = new C
            {
                Identity = new A
                {
                    Name = "ORTSIGNINGPRO"
                }
            };
            var ctx = new DbSqlContext("ORTSigningProEntities");

            var result = ctx.From<VendorPortalAccount>().FirstOrDefault(w => w.UserName == x.Identity.Name);

            if (result != null)
            {

            }
        }
    }

    class C
    {
        public A Identity { get; set; }
    }

    class A
    {
        public string Name { get; set; }
    }
}
