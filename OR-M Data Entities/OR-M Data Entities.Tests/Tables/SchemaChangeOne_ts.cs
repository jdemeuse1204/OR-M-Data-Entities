using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables
{
    [Schema("ts")]
    [Table("SchemaChangeOne")]
    public class SchemaChangeOne_ts
    {
        [DbGenerationOption(DbGenerationOption.Generate)]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
