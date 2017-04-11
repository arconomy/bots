using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class AppleaDay : Robot
    {
        [Parameter("Contract Size", DefaultValue = 0.0)]
        public int ContractSize { get; set; }


        protected override void OnStart()
        {
            // Put your initialization logic here



            double balance = Account.Balance;
            string currency = Account.Currency;
            double equity = Account.Equity;
            double freemargin = Account.FreeMargin;
            double margin = Account.Margin;
            double? marginlevel = Account.MarginLevel;
            double leverage = Account.PreciseLeverage;

            Print("Balance " + balance.ToString());
            Print("Currency " + currency.ToString());
            Print("Equity: " + equity.ToString());
            Print("freemargin: " + freemargin.ToString());
            Print("margin: " + margin.ToString());
            Print("marginlevel: " + marginlevel.ToString());
            Print("leverage: " + leverage.ToString());

            double SL = 50;
            double TP = 50;

            double multiplier = (1 / Symbol.TickSize);
            double PointValue = Symbol.TickValue * multiplier;
            double Profit = PointValue * SL;
            Print("Tick Size: " + Symbol.TickSize.ToString());
            Print("Tick Value: " + Symbol.TickValue.ToString());
            Print("Take Profit $: " + Profit.ToString());

            double BarLength = Math.Abs(MarketSeries.High.Last(8) - MarketSeries.Low.Last(8));
            //     BarLength = Math.Round(BarLength, 0);
            if (BarLength < 1)
                BarLength = 1;

            Print("Bar Length: " + BarLength.ToString());

            double Entry = 7350;

            var Result = PlaceStopOrder(TradeType.Buy, Symbol, 1, Entry, "BarBreak", BarLength, BarLength);

            if (Result.IsSuccessful)
            {

            }
            else
            {
                Print("Error: " + Result.Error);
            }
        }


        protected override void OnBar()
        {






        }


        protected override void OnTick()
        {




        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
