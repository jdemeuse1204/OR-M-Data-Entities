using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Tests.Tables.EntityStateTrackableOn
{
    [Table("SchemaChangeOne")]
    public class SchemaChangeOne_dbo : EntityStateTrackable
    {
        [DbGenerationOption(DbGenerationOption.Generate)]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
