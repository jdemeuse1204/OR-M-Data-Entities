using System;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOff
{
    public class Album
    {
        [DbGenerationOption(DbGenerationOption.Generate)]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public int TimesDownloaded { get; set; }

        public int ArtistId { get; set; }
    }
}
