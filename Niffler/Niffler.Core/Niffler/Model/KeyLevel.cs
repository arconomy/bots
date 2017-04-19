using System;

namespace Niffler.Model
{ 
    public partial class KeyLevel
    { 
        public string BrokerName { get; set; }
         
        public string Symbol { get; set; }
         
        public DateTime Date { get; set; }
         
        public decimal? High { get; set; }
         
        public decimal? Low { get; set; }
         
        public decimal? Close { get; set; }
         
        public decimal? Open { get; set; }
    }
}
