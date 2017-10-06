using System;

namespace Niffler.Model
{ 
    public partial class Position
    { 
        public int ID { get; set; }

        public int? AccountID { get; set; }
         
        public string Symbol { get; set; }
         
        public string TradeType { get; set; }

        public long? Volume { get; set; }

        public double? Quantity { get; set; }
         
        public decimal? EntryPrice { get; set; }
         
        public decimal? StopLoss { get; set; }
         
        public decimal? TakeProfit { get; set; }
         
        public decimal? ClosedPrice { get; set; }
         
        public decimal? Swap { get; set; }
         
        public string Channel { get; set; }
         
        public string Label { get; set; }
         
        public string Comment { get; set; }
         
        public decimal? GrossProfit { get; set; }
         
        public decimal? NetProfit { get; set; }

        public double? Pips { get; set; }

        public DateTime? DateTimeCreatedUTC { get; set; }

        public DateTime? DateTimeLastModifiedUTC { get; set; }
         
        public string Status { get; set; }
    }
}
