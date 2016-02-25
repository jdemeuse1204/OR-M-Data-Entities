using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    public class Car : EntityStateTrackable
    {
        public Car()
        {
        }

        public Car(string make)
        {
            Make = make;
        }

        public int ID { get; set; }

        public string Name { get; set; }

        public string Make { get; set; }

        public string Model { get; set; }

        public string Trim { get; set; }

        public int Horsepower { get; set; }
    }
}
