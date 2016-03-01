using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Scripts;
using OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOff;

namespace OR_M_Data_Entities.Tests.StoredSql
{
    public class ScalarFunction1 : ScalarFunction<int>
    {

    }

    public class CustomScript2 : CustomScript
    {
        protected override string Sql
        {
            get { return "Select Top 1 1"; }
        }
    }

    public class CustomScript1 : CustomScript<int>
    {
        protected override string Sql
        {
            get { return "Select Top 1 1"; }
        }
    }

    public class CS1 : CustomScript<Contact>
    {
        public int Id { get; set; }

        protected override string Sql
        {
            get { return @"

                    Select Top 1 * From Contacts Where Id = @Id

                "; }
        }
    }

    public class CS2 : CustomScript
    {
        public int Id { get; set; }

        public string Changed { get; set; }

        protected override string Sql
        {
            get { return @"

                    Update Contacts Set LastName = @Changed Where Id = @Id

                "; }
        }
    }

    [Script("GetLastName")]
    [ScriptPath("../../Scripts2")]
    public class SS1 : StoredScript<Contact>
    {
        public int Id { get; set; }
    }

    [Script("GetFirstName")]
    public class SS2 : StoredScript<Contact>
    {
    }

    [Script("UpdateFirstName")]
    public class SS3 : StoredScript
    {
        public string FirstName { get; set; }

        public int Id { get; set; }
    }

    [Script("GetFirstName")]
    public class SP1 : StoredProcedure<Contact>
    {
        public int Id { get; set; }
    }

    [Script("UpdateFirstName")]
    [Schema("dbo")]
    public class SP2 : StoredProcedure
    {
        [Index(1)]
        public int Id { get; set; }

        [Index(2)]
        public string FirstName { get; set; }
    }

    [Script("GetLastName")]
    public class SF1 : ScalarFunction<string>
    {
        [Index(2)]
        public int Id { get; set; }

        [Index(1)]
        public string FirstName { get; set; }
    }
}
