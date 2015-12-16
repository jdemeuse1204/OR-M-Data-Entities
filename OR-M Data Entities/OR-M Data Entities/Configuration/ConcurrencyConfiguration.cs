using OR_M_Data_Entities.Enumeration;

namespace OR_M_Data_Entities.Configuration
{
    public sealed class ConcurrencyConfiguration
    {
        public bool IsOn { get; set; }

        public ConcurrencyViolationRule ViolationRule { get; set; }
    }
}
