using System;
using System.Collections.Generic;
using System.Linq;
using ORSigningPro.Common.Data.ORTSigningPro.Tables;
using OR_M_Data_Entities;
using OR_M_Data_Entities.Tests.Tables;

namespace LambdaResolver
{
    class Program
    {
        static void Main(string[] args)
        {
            var q = new Queue<int>();

            q.Enqueue(1);
            q.Enqueue(2);

            var ctx = new DbSqlContext("ORTSigningProEntities");

            var sdf = ctx.Find<MobileClosing>(1);

            if (sdf != null)
            {
                
            }

            var s = DateTime.Now;
            var x = ctx.FromView<MobileClosing>("ActiveOrder").All(w => w.VincaVendorID == 33159);
            var e = DateTime.Now;
            var f = e - s;

            s = DateTime.Now;
            var product = ctx.From<Product>()
                .First<Product>(w => w.ProductNumber == "03N" && w.Order.OrderNumber == "01-15000317");
            e = DateTime.Now;
            f = e - s;

            if (x != null && f.Days != 0 && product != null)
            {
                
            }
        }
    }
}
