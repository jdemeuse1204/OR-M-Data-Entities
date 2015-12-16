using System.Collections.Generic;
using ORSigningPro.Common.Data.Tables.Vinca;

namespace ORSigningPro.Common.BusinessObjects.Order.Base
{
    public class OrderDetails
    {
        private List<string> _options = new List<string>();
        
        public string Title { get; set; }

        public string HomePhone { get; set; }

        public string WorkPhone { get; set; }

        public string MobilePhone { get; set; }

        public string Email { get; set; }

        public decimal Distance { get; set; }

        public decimal Price { get; set; }

        public List<string> Options
        {
            get { return _options; }
            set { _options = value ?? new List<string>(); }
        }

        public int CountyCode { get; set; }

        public string StateCode { get; set; }

        public Address PropertyAddress { get; set; }
    }
}
