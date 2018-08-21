using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("Webhooks")]
    public class ReadOnlyWebhook
    {
        [Key]
        public int Id { get; set; }
        public string Url { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }
        public DateTimeOffset UpdatedDateTime { get; set; }
        public int EventId { get; set; }
        public Guid ApiTokenId { get; set; }
        public int SourceSystemId { get; set; }

        [ForeignKey("EventId")]
        public ReadOnlyWebhookEvent Event { get; set; }

        [ForeignKey("ApiTokenId")]
        public ReadOnlyApiToken ApiToken { get; set; }

        [ForeignKey("WebhookId")]
        public List<ReadOnlyWebhookHeader> Headers { get; set; }
    }
}
