using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("Images")]
    public class ReadOnlyImage
    {
        [Key]
        public int Id { get; set; }
        public string FileNameAndPath { get; set; }
        public DateTimeOffset UploadedDateTime { get; set; }
        public Guid UploadedByUserId { get; set; }
        [Unmapped]
        public string Data { get; set; }

        public void Clean()
        {
            FileNameAndPath = "";
        }
    }
}
