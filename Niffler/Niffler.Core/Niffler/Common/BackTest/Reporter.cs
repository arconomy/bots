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
using Niffler.Rules;

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
        private List<string> RulesExecuted = new List<string>();
        private string BotName;
        private string ReportDirectory;
        private string ReportFile;

        public Reporter(State s, SpikeManager spikeManager)
        {
            BotState = s;
            Bot = BotState.Bot;
            SpikeManager = spikeManager;
            BotName = Bot.GetType().Name;
            ReportDirectory = "C:\\Users\\alist\\Desktop\\" + BotName;
            ReportFile = "C:\\Users\\alist\\Desktop\\" + BotName + "\\" + BotName + "-" + BotState.GetMarketName() + "-" + BotState.BotId + "-" + Utils.GetTimeStamp(true) + ".csv";
            Reset();
        }

        public void Reset()
        {
            // reset reporting variables
            ProfitTotal = 0;
            PipsTotal = 0;
            TradeResults.Clear();
            TotalsResults.Add(GetReportTradesHeaders());
            TotalsResults.Clear();
            TotalsResults.Add(GetReportTotalsHeaders());
            RulesExecuted.Clear();
            RulesExecuted.Add(GetReportRulesHeaders());
        }

        private string GetReportTradesHeaders()
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

        public void ReportTrade(Position p, String LastStopLossRule)
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
                BotState.GetReportSnapShot()
                );
            AddTradeToTotals(p);
        }

        public void LogRuleExecution(IRule rule)
        {
            RulesExecuted.Add(System.DateTime.Now + "," + rule.GetType().Name);

            //Add rule logging to TradeResults rather than RulesExecuted to see the Rules that were executed just prior to a position closing
            WriteCSVFile(TradeResults);
        }

        private string GetReportRulesHeaders()
        {
            return "Date/Time" + "," + "Rule" + "," + "Execution Count";
        }

        public void ReportRuleExecutionResults(IRule rule, int executionCount)
        {
            RulesExecuted.Add(System.DateTime.Now + "," + rule.GetType().Name + "," + executionCount);
        }

        private void AddTradeToTotals(Position p)
        {
            ProfitTotal += p.GrossProfit;
            PipsTotal += p.Pips;
        }

        private string GetReportTotalsHeaders()
        {
            return "Profit," +
            "Pips" +
            ",Day" +
            ",Opened Positions" +
            ",Spike Peak" +
            ",Day" +
            ",Date/Time";
        }

        public void Report()
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

            WriteCSVFile(TradeResults);
            WriteCSVFile(TotalsResults);
            WriteCSVFile(RulesExecuted);
        }

        public void ReportTradeResultError(string msg)
        {
            TradeResults.Add(msg);
        }


        private void WriteCSVFile(List<String> data)
        {
            if (!System.IO.Directory.Exists(ReportDirectory))
                System.IO.Directory.CreateDirectory(ReportDirectory);

            if(System.IO.File.Exists(ReportFile))
            {
                System.IO.File.AppendAllLines(ReportFile, data.ToArray());
            }
            else
            {
                System.IO.File.WriteAllLines(ReportFile, data.ToArray());
            }
        }



    }
}
