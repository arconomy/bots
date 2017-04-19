using System;

namespace Niffler.Model
{
    
    public partial class Account
    { 
        public int ID { get; set; }
         
        public string SlackID { get; set; }
         
        public string Name { get; set; }
         
        public decimal? Balance { get; set; }
         
        public string Currency { get; set; }
         
        public string BrokerName { get; set; }
         
        public decimal? Equity { get; set; }

        public bool? IsLive { get; set; }
         
        public decimal? Margin { get; set; }

        public float? MarginLevel { get; set; }

        public float? PreciseLeverage { get; set; }
         
        public decimal? UnrealizedGrossProfit { get; set; }
         
        public decimal? UnrealizedNetProfit { get; set; }
         
        public string Status { get; set; }

        public DateTime DateTimeLastModifiedUTC { get; set; }
    }
}
