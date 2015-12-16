using System;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace ORSigningPro.Common.Data.Tables.SigningPro
{
    [Table("aspnet_JSONWebTokens")]
    public class aspnet_JSONWebToken : EntityStateTrackable
    {
        [DbGenerationOption(DbGenerationOption.Generate)]
        public Guid Id { get; set; }

        public Guid IssuedUserId { get; set; }

        public DateTime IssuedDate { get; set; }

        public bool IsBlackListed { get; set; }
    }
}
