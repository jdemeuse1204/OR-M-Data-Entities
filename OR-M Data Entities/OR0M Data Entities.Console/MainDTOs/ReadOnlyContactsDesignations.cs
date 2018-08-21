using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("ContactsDesignations")]
    public class ReadOnlyContactsDesignations
    {
        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public Guid ContactId { get; set; }

        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public int DesignationId { get; set; }

        [ForeignKey("DesignationId")]
        public ReadOnlyDesignation Designation { get; set; }
    }
}
