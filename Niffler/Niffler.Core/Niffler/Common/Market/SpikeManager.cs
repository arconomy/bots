using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo;
using cAlgo.API;
using cAlgo.API.Internals;

namespace Niffler.Common.Market
{
    class SpikeManager
    {
        protected double SpikePeakPips = 0;
        protected double SpikePeakPrice = 0;
        public bool SpikePeakCaptured { get; set; }
        private State BotState;
        private Robot Bot;

        public SpikeManager(Robot r, State s)
        {
            Bot = r;
            BotState = s;
            SpikePeakCaptured = false;
        }

        //TO DO: Refactor where the a Spike Object is created and managed.
        public void captureSpike()
        {
            //Capture the highest point of the Spike within Swordfish Time
            if (BotState.OpenedPositionsCount > 0)
            {
                switch (BotState.LastPositionTradeType)
                {
                    case TradeType.Buy:
                        {
                            //If we are buying then spike is down so look for prices less than current spikePeakPrice
                            if (Bot.Symbol.Bid < SpikePeakPrice || SpikePeakPrice == 0)
                            {
                                SpikePeakPrice = Bot.Symbol.Bid;
                                SpikePeakPips = BotState.OpenPrice - Bot.Symbol.Bid;
                            }
                            break;
                        }
                    case TradeType.Sell:
                        {
                            //If we are selling then spike is up so look for prices more than current spikePeakPrice
                            if (Bot.Symbol.Ask > SpikePeakPrice || SpikePeakPrice == 0)
                            {
                                SpikePeakPrice = Bot.Symbol.Ask;
                                SpikePeakPips = Bot.Symbol.Ask - BotState.OpenPrice;
                            }

                            break;
                        }
                }
            }
            if (SpikePeakPips > 0)
                SpikePeakCaptured = true;
        }

        //Return the greater retrace of the percentage price or percent closed positions
        protected double calculateRetraceFactor()
        {
            double retraceFactor = 0;
            double percentClosed = calculatePercentageClosed();
            double percentRetrace = calculatePercentageRetrace();
            if (percentClosed <= percentRetrace)
            {
                retraceFactor = percentRetrace;
            }
            else
            {
                retraceFactor = percentClosed;
            }
            return retraceFactor;
        }

        protected double calculatePercentageRetrace()
        {
            double percentRetrace = 0;
            if (BotState.LastPositionTradeType == TradeType.Sell)
            {
                //Position are Selling
                percentRetrace = (Symbol.Bid - _openPrice) / (BotState.LastPositionEntryPrice - _openPrice);
            }

            if (BotState.LastPositionTradeType == TradeType.Buy)
            {
                //Positions are buying
                percentRetrace = (_openPrice - Symbol.Ask) / (_openPrice - BotState.LastPositionEntryPrice);
            }

            percentRetrace = 1 - percentRetrace;
            percentRetrace = percentRetrace * 100;

            return percentRetrace;
        }



    }
}
