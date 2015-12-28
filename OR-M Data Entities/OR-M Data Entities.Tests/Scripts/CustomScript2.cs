using OR_M_Data_Entities.Scripts;

namespace OR_M_Data_Entities.Tests.Scripts
{
    public class CustomScript2 : CustomScript
    {
        protected override string Sql
        {
            get { return "Select Top 1 1"; }
        }
    }
}
