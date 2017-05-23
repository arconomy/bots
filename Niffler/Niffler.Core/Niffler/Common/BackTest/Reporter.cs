using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo;
using cAlgo.API;
using cAlgo.API.Internals;
using Niffler.Common.Market;
using Niffler.Common.Trade;

namespace Niffler.Common.BackTest
{
    class Reporter : IResetState
    {

        private State BotState;
        private Robot Bot;
        private SpikeManager SpikeManager;
        private double ProfitTotal = 0;
        private double PipsTotal = 0;
        private List<string> TradeResults = new List<string>();
        private List<string> TotalsResults = new List<string>();

        public Reporter(State s, SpikeManager spikeManager)
        {
            BotState = s;
            Bot = BotState.Bot;
            SpikeManager = spikeManager;
        }

        public void Reset()
        {
            ReportTotals();

            // reset reporting variables
            ProfitTotal = 0;
            PipsTotal = 0;

        }


        public void ReportTrade(Position p, String StopLossStatus)
        {
            TradeResults.Add(
                p.Label + "," +
                p.GrossProfit + "," +
                p.Pips + "," +
                p.EntryPrice + "," +
                Bot.History.FindLast(p.Label, Bot.Symbol, p.TradeType).ClosingPrice + "," +
                p.StopLoss + "," +
                p.TakeProfit + "," +
                System.DateTime.Now.DayOfWeek +
                System.DateTime.Now +
                StopLossStatus +
                BotState.GetReportSnapShot()
                );

            AddTradeToTotals(p);
            Utils.WriteCSVFile(BotState, "niffler", "teddy", TradeResults);
        }


        private void AddTradeToTotals(Position p)
        {
            ProfitTotal += p.GrossProfit;
            PipsTotal += p.Pips;
        }

        public string GetReportTradesHeaders()
        {

            return "Label," +
            "Profit," +
            "Pips" +
            ",EntryPrice" +
            ",ClosePrice" +
            ",SL" +
            ",TP" +
            ",Day" +
            ",Date/Time" +
            BotState.GetReportSnapShotHeaders();
        }

        public void ReportTotals()
        {

            if (ProfitTotal != 0 && PipsTotal != 0)
            {
                TotalsResults.Add(
                    ProfitTotal + "," +
                    PipsTotal + "," +
                    BotState.OpenedPositionsCount + "," +
                    SpikeManager.GetSpikePeakPips() + "," +
                    System.DateTime.Now.DayOfWeek + "," +
                    System.DateTime.Now
                    );
            }

            Utils.WriteCSVFile(BotState, "niffler", "teddy", TotalsResults);
        }

        public string GetReportTotalsHeaders()
        {
            return "Profit," +
            "Pips" +
            ",Day" +
            ",Opened Positions" +
            ",Spike Peak" +
            ",Day" +
            ",Date/Time";
        }
    }
}
