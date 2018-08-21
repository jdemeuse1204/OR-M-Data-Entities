using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("Addresses")]
    public class ReadOnlyAddress
    {
        [Key]
        public int AddressId { get; set; }
        public string Name { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string AptSuiteNumber { get; set; }
        public int StateId { get; set; }
        public int AddressTypeId { get; set; }
        public int DwellingTypeId { get; set; }
        public string GooglePlaceId { get; set; }

        [ForeignKey("DwellingTypeId")]
        public ReadOnlyDwellingType DwellingType { get; set; }

        [ForeignKey("AddressTypeId")]
        public ReadOnlyAddressType AddressType { get; set; }

        [ForeignKey("StateId")]
        public ReadOnlyState State { get; set; }
    }
}
