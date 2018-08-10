using OR_M_Data_Entities.Lite.Mapping;
using System;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM
{
    [Table("ContactsDesignations")]
    public class ReadOnlyContactsDesignations
    {
        [Key]
        public Guid ContactId { get; set; }

        [Key]
        public int DesignationId { get; set; }

        [ForeignKey("DesignationId")]
        public ReadOnlyDesignation Designation { get; set; }
    }
}
