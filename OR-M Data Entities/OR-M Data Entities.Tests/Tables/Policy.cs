using System;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables
{
    [Table("Policy")]
    public class Policy
    {
        [Column("PolicyID")]
        public int Id { get; set; }

        public int FileNumber { get; set; }

        [Column("PolicyTypeID")]
        public int PolicyInfoId { get; set; }

        public int? StateID { get; set; }

        public string County { get; set; }

        public DateTime CreatedDate { get; set; }

        public string FeeOwnerName { get; set; }

        public string InsuredName { get; set; }

        public decimal? PolicyAmount { get; set; }

        public DateTime? PolicyDate { get; set; }

        public string PolicyNumber { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string CreatedBy { get; set; }

        public string UpdatedBy { get; set; }
    }
}
