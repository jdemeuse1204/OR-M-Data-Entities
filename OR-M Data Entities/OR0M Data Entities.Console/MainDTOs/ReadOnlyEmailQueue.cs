using OR_M_Data_Entities.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Main
{
    [ReadOnly(ReadOnlySaveOption.Skip)]
    [Table("Emails")]
    public class ReadOnlyEmailQueue
    {
        [Key]
        public Guid Id { get; set; }
        public string To { get; set; }
        public string Bcc { get; set; }
        public string Cc { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsBodyHtml { get; set; }
        public DateTimeOffset? SentDateTime { get; set; }
        public DateTimeOffset SendDateTime { get; set; }
        public int RetryCount { get; set; }
    }
}
