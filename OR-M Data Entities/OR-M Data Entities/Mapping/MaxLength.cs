using System;
using OR_M_Data_Entities.Enumeration;

namespace OR_M_Data_Entities.Mapping
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class MaxLengthAttribute : Attribute
    {
        public MaxLengthAttribute(int length, MaxLengthViolationType violationType = MaxLengthViolationType.Truncate)
        {
            Length = length;
            ViolationType = violationType;
        }

        public int Length { get; private set; }

        public MaxLengthViolationType ViolationType { get; private set; }
    }
}
