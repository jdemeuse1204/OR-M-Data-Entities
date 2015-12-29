using System;
using OR_M_Data_Entities.Tests.Tables;
using OR_M_Data_Entities.Tests.Testing.Context;

namespace OR_M_Data_Entities.Tests.Testing.Base
{
    public static class OtherTests
    {
        public static bool Test_1(InsertKeyChangeContext ctx)
        {
            try
            {
                var policy = new Policy
                {
                    Id = -1,
                    County = "Hennepin",
                    CreatedBy = "James Demeuse",
                    CreatedDate = DateTime.Now,
                    FeeOwnerName = "Test",
                    FileNumber = 100,
                    InsuredName = "James Demeuse",
                    PolicyAmount = 100,
                    PolicyDate = DateTime.Now,
                    PolicyInfoId = 1,
                    PolicyNumber = "001-8A",
                    StateID = 7,
                    UpdatedBy = "Me",
                    UpdatedDate = DateTime.Now
                };

                ctx.SaveChanges(policy);

                return policy.Id != -1;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
