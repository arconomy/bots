using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using Niffler.Common;

namespace Niffler.Rules
{
    class OpenTimeUseBollingerBand : IRule
    {
        public OpenTimeUseBollingerBand(int priority) : base(priority) { }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        //After CLose time set hard stop losses at last position entry price with Buffer
        override protected void Execute()
        {
            if (MarketInfo.IsBotTradingOpen())
            {
                //SellLimitOrdersTrader.SetBollingerBandDefault(Bot.MarketData.GetSeries());

                //Price moves TriggerOrderPlacementPips UP from open then look to set SELL LimitOrders
                if (BotState.OpenPrice + SellLimitOrdersTrader.EntryTriggerOrderPlacementPips < Bot.Symbol.Bid)
                {
                    if(SellLimitOrdersTrader.IsOutSideBollingerBand())
                    {
                        SellLimitOrdersTrader.PlaceSellLimitOrders();
                        BotState.OrdersPlaced = true;
                        ExecuteOnceOnly();
                    }
                   
                }
                //Price moves 5pts DOWN from open then look to set BUY LimitOrders
                else if (BotState.OpenPrice - BuyLimitOrdersTrader.EntryTriggerOrderPlacementPips > Bot.Symbol.Ask)
                {
                    if (BuyLimitOrdersTrader.IsOutSideBollingerBand())
                    {
                        BuyLimitOrdersTrader.PlaceBuyLimitOrders();
                        BotState.OrdersPlaced = true;
                        ExecuteOnceOnly();
                    }
                }
            }
        }

        override protected void Reset()
        {
            // reset any botstate variables to the state prior to executing rule
            SellLimitOrdersTrader.ResetBollingerBand();
            BuyLimitOrdersTrader.ResetBollingerBand();
        }

        override public void Report()
        {
            // report stats on rule execution 
            // e.g. execution rate, last position rule applied to, number of positions impacted by rule
        }
    }
}
