using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("WebhooksHeaders")]
    public class ReadOnlyWebhookHeader
    {
        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public int WebhookId { get; set; }

        [Key]
        [DbGenerationOption(DbGenerationOption.None)]
        public int HeaderId { get; set; }

        [ForeignKey("HeaderId")]
        public ReadOnlyHeader Header { get; set; }
    }
}
