using System;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn
{
    public class Album : EntityStateTrackable
    {
        [DbGenerationOption(DbGenerationOption.Generate)]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public int TimesDownloaded { get; set; }

        public int ArtistId { get; set; }
    }
}
