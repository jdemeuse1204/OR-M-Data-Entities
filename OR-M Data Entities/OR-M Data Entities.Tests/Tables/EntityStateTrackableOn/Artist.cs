using System;
using System.Collections.Generic;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn
{
    public class Artist : EntityStateTrackable
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Genre { get; set; }

        public DateTime ActiveDate { get; set; }

        public int? RecordLabelId { get; set; }

        [PseudoKey("RecordLabelId", "Id")]
        //[ForeignKey("RecordLabelId")]
        public RecordLabel RecordLabel { get; set; }

        public int AgentId { get; set; }

        [PseudoKey("AgentId", "Id")]
        //[ForeignKey("AgentId")]
        public Agent Agent { get; set; }

        [PseudoKey("Id", "ArtistId")]
        //[ForeignKey("ArtistId")]
        public List<Album> Albums { get; set; }
    }
}
