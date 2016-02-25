using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn
{
    public class Processor : EntityStateTrackable
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Cores { get; set; }

        public CoreType? CoreType { get; set; }

        public int? Speed { get; set; }
    }
}
