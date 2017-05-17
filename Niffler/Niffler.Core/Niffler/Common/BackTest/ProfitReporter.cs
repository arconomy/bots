﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo;
using cAlgo.API;
using cAlgo.API.Internals;

namespace Niffler.Common.BackTest
{
    class ProfitReporter
    {

        private State BotState { get; set; }
        private Robot Bot { get; set; }
        private double ProfitTotal = 0;
        private double PipsTotal = 0;
        private List<string> TradeResults = new List<string>();
        private List<string> TotalsResults = new List<string>();

        public ProfitReporter(State s)
        {
            BotState = s;
            Bot = BotState.Bot;
        }


        public void reportTrade(Position p)
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
                BotState.getStateSnapShot()
                );

            addTradeToTotals(p);
        }


        private void addTradeToTotals(Position p)
        {
            ProfitTotal += p.GrossProfit;
            PipsTotal += p.Pips;
        }

        public string getReportTradesHeaders()
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
            BotState.getStateSnapShotHeaders();
        }

        public void reportTotals(State botState)
        {
               
                if (ProfitTotal != 0 && PipsTotal != 0)
                {
                     TotalsResults.Add(
                         ProfitTotal + "," + 
                         PipsTotal + "," + 
                         botState.OpenedPositionsCount + "," +
                         _spikePeakPips + "," + 
                         System.DateTime.Now.DayOfWeek + "," + 
                         System.DateTime.Now
                         );
                }


                // reset reporting variables
                ProfitTotal = 0;
                PipsTotal = 0;
                
                _spikePeakPips = 0;
                _spikePeakPrice = 0;
            }
        }

    public string getReportTotalsHeaders()
    {
        return "Profit," +
        "Pips" +
        ",Day" +
        ",Opened Positions" +
        ",Spike Peak" +
        ",Day" +
        ",Date/Time");
    }


   


}
}
