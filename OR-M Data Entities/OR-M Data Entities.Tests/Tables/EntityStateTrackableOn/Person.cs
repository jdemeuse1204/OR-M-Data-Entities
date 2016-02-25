using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn
{
    [ReadOnly(ReadOnlySaveOption.ThrowException)]
    public class Person : EntityStateTrackable
    {
        public int ID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string StreetAddress { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Zip { get; set; }

        public int CarID { get; set; }
    }
}
