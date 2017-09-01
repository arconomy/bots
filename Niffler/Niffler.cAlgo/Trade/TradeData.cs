using cAlgo.API;
using cAlgo.API.Internals;


namespace Niffler.Common.Trade
{
    class TradeData
    {
        public TradeType tradeType { get; set; }
        public Symbol symbol { get; set; }
        public int volume { get; set; }
        public double entryPrice { get; set; }
        public string label { get; set; }
        public double stopLossPips { get; set; }
        public double takeProfitPips { get; set; }
    }
}
