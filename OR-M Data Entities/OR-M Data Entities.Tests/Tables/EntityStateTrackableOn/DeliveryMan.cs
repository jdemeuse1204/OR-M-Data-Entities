using System;
using System.Data;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn
{
    public class DeliveryMan : EntityStateTrackable
    {
        [DbGenerationOption(DbGenerationOption.Generate)]
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int AverageDeliveryTime { get; set; }

        [DbType(SqlDbType.Timestamp)]
        public byte[] CreateDate { get; set; }
    }
}
