﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo;
using cAlgo.API;
using cAlgo.API.Internals;

namespace Niffler.Common.Market
{
    class SpikeManager : IResetState
    {
        private State BotState;
        private Robot Bot;
        private Spike Spike;
        public int Level1 { get; set; }
        public int Level2 { get; set; }
        public int Level3 { get; set; }
        private int RetraceFactor { get; set; }

        public SpikeManager(State s)
        {
            BotState = s;
            Bot = BotState.Bot;
            Level1 = 33;
            Level2 = 50;
            Level3 = 66;
        }

        public bool IsRetraceBetweenLevel1AndLevel2()
        {
            return Level1 < RetraceFactor && RetraceFactor < Level2;
        }

        public bool IsRetraceBetweenLevel2AndLevel3()
        {
            return Level2 < RetraceFactor && RetraceFactor < Level3;
        }

        public bool IsRetraceGreaterThanLevel3()
        {
            return Level3 < RetraceFactor;
        }

        public bool IsRetraceLessThanLevel1()
        {
            return Level1 > RetraceFactor;
        }

        public void reduceLevelsBy50Percent()
        {
            Level1 /= 2;
            Level2 /= 2;
            Level3 /= 2;
        }

        public double getSpikePeakPips()
        {
            if(isSpikeCaptured())
            {
                return Spike.PeakPips;
            }
            else
            {
                return 0;
            }
            
        }

        public double getSpikePeakPrice()
        {
            if (isSpikeCaptured())
            {
                return Spike.PeakPrice;
            }
            else
            {
                return 0;
            }

        }

        public void reset()
        {
            Spike = null;
        }

        public bool isSpikeCaptured()
        {
            return Spike.isCaptured();
        }

        //TO DO: Refactor where the a Spike Object is created and managed.
        public void captureSpike()
        {
            if(Spike == null)
            {
                Spike = new Spike();
            }

            //Capture the highest point of the Spike within Swordfish Time
            if (BotState.OpenedPositionsCount > 0)
            {
                switch (BotState.LastPositionTradeType)
                {
                    case TradeType.Buy:
                        {
                            //If we are buying then spike is down so look for prices less than current spikePeakPrice
                            if (Bot.Symbol.Bid < Spike.PeakPrice || !Spike.isCaptured())
                            {
                                Spike.setPeak(Bot.Symbol.Bid, BotState.OpenPrice - Bot.Symbol.Bid);
                            }
                            break;
                        }
                    case TradeType.Sell:
                        {
                            //If we are selling then spike is up so look for prices more than current spikePeakPrice
                            if (Bot.Symbol.Ask > Spike.PeakPrice || !Spike.isCaptured())
                            {
                                Spike.setPeak(Bot.Symbol.Ask, Bot.Symbol.Ask - BotState.OpenPrice);
                            }
                            break;
                        }
                }
            }
        }

        //Return the greater retrace of the percentage price or percent closed positions
        public void calculateRetraceFactor()
        {
            double retraceFactor = 0;
            double percentClosed = BotState.calcPercentOfPositionsClosed();
            double percentRetrace = calculatePercentageRetrace();
            if (percentClosed <= percentRetrace)
            {
                retraceFactor = percentRetrace;
            }
            else
            {
                retraceFactor = percentClosed;
            }
            RetraceFactor = (int) retraceFactor;
        }

        protected double calculatePercentageRetrace()
        {
            double percentRetrace = 0;
            if (BotState.LastPositionTradeType == TradeType.Sell)
            {
                //Position are Selling
                percentRetrace = (Bot.Symbol.Bid - BotState.OpenPrice) / (Spike.PeakPrice - BotState.OpenPrice);
            }

            if (BotState.LastPositionTradeType == TradeType.Buy)
            {
                //Positions are buying
                percentRetrace = (BotState.OpenPrice - Bot.Symbol.Ask) / (BotState.OpenPrice - Spike.PeakPrice);
            }

            percentRetrace = 1 - percentRetrace;
            percentRetrace = percentRetrace * 100;

            return percentRetrace;
        }



    }
}