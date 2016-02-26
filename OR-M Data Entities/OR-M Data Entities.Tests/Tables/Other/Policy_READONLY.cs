using System;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn
{
    [ReadOnly(ReadOnlySaveOption.ThrowException)]
    [Table("Policy")]
    public class Policy_READONLY : EntityStateTrackable
    {
        [Column("PolicyID")]
        public int Id { get; set; }

        public int FileNumber { get; set; }

        [Column("PolicyTypeID")]
        public int PolicyInfoId { get; set; }

        public int? StateID { get; set; }

        public string County { get; set; }

        public DateTime CreatedDate { get; set; }

        [MaxLength(10, MaxLengthViolationType.Error)]
        public string FeeOwnerName { get; set; }

        [MaxLength(10)]
        public string InsuredName { get; set; }

        public decimal? PolicyAmount { get; set; }

        public DateTime? PolicyDate { get; set; }

        public string PolicyNumber { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string CreatedBy { get; set; }

        public string UpdatedBy { get; set; }

        [Unmapped]
        public string Test { get; set; }
    }
}
