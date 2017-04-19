using System;

namespace Niffler.Model
{
  
    public partial class TradingHour
    { 
        public string Symbol { get; set; }
         
        public string Type { get; set; }
         
        public string Name { get; set; }
         
        public string OpenTimeUTC { get; set; }
         
        public string CloseTimeUTC { get; set; }
         
        public string TimeZone { get; set; }
         
        public string DaysOpen { get; set; }
    }
}
