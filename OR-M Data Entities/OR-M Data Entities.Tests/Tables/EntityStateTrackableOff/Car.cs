using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOff
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    public class Car
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
