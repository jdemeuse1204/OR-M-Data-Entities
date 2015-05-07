using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities;
using OR_M_Data_Entities.Expressions;
using OR_M_Data_Entities.Tests.Tables;

namespace LambdaResolver
{
    class Program
    {
        static void Main(string[] args)
        {
            var ctx = new DbSqlContext("sqlExpress");

            var lst = new List<int> { 1, 2, 3, 4, 5 };
            var item = ctx.From<Contact>().Where(
                w =>
                    w.ID == 1 && w.FirstName == "James" ||
                    w.FirstName == "Megan" && w.FirstName == "WIN" && w.FirstName == "AHHHH" ||
                    w.FirstName == "" &&
                    w.ID == ctx.From<Appointment>().Where(z => z.Description == "").Select(x => x.ContactID).First());
        }
    }
}
