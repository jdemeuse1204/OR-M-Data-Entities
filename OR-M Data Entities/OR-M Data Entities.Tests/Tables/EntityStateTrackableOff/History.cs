using System.Data;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOff
{
    [LookupTable("History")]
    public class History
    {
        public int Id { get; set; }

        [DbType(SqlDbType.Timestamp)]
        public byte[] CreateDate { get; set; }

        public string Description { get; set; }

        public int ComputerId { get; set; }
    }
}
