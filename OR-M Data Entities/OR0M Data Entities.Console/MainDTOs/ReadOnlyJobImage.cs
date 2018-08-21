using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("JobImages")]
    public class ReadOnlyJobImage
    {
        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public int JobId { get; set; }

        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public int ImageId { get; set; }

        [ForeignKey("ImageId")]
        public ReadOnlyImage Image { get; set; }
    }
}
