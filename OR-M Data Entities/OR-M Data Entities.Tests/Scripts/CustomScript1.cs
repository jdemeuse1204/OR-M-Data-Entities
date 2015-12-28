using OR_M_Data_Entities.Scripts;

namespace OR_M_Data_Entities.Tests.Scripts
{
    public class CustomScript1 : CustomScript<int>
    {
        protected override string Sql
        {
            get { return "Select Top 1 1"; }
        }
    }
}
