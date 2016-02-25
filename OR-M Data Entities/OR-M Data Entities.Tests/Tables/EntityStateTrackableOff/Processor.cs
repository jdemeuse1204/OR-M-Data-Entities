namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOff
{
    public class Processor
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Cores { get; set; }

        public CoreType? CoreType { get; set; }

        public int? Speed { get; set; }
    }
}
