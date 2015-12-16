using System;
using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.Tables.Vinca
{
    [ReadOnly(ReadOnlySaveOption.ThrowException)]
    [LinkedServer("ORSIGNING", "OVM", "DBO")]
    public class AncillaryProduct
    {
        [Key]
        public int ProductID { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        public DateTime? ActualAppointmentDate { get; set; }
        
        public DateTime? ActualVerbalDate { get; set; }
        
        public DateTime? ActualHardcopyDate { get; set; }
        
        public byte[] LastChanged { get; set; }
        
        public bool? IsActualAppointment { get; set; }
        
        public bool? IsActualVerbal { get; set; }
        
    }
}
