using System;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOff
{
    public class PolicyInfo
    {
        [DbGenerationOption(DbGenerationOption.Generate)]
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Description { get; set; }

        public Guid Stamp { get; set; }
    }
}
