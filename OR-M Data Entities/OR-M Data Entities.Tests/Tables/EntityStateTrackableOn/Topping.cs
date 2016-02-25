using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn
{
    public class Topping : EntityStateTrackable
    {
        public int Id { get; set; }

        public decimal Cost { get; set; }

        public string Name { get; set; }
    }
}
