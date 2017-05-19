using System;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using Niffler.Common;
using Niffler.Common.TrailingStop;
using Niffler.Common.Market;
using Niffler.Common.Trade;
using Niffler.Common.BackTest;
using Niffler.Rules;

namespace Niffler.Bots.Swordfish

{
    public class swf : cAlgo.API.Robot
    { 
        [Parameter("Source")]
        public DataSeries DataSeriesSource { get; set; }

        [Parameter("Use Bollinger Bollinger Band Entry", DefaultValue = false)]
        public bool UseBollingerBandEntry { get; set; }

        [Parameter("Pips inside Bollinger Band for Entry", DefaultValue = 2)]
        public int BolliEntryPips { get; set; }

        [Parameter("Initial Order placement trigger from open", DefaultValue = 5)]
        public int TriggerOrderPlacementPips { get; set; }

        [Parameter("Offset from Market Open for First Order", DefaultValue = 9)]
        public int OrderEntryOffset { get; set; }

        [Parameter("Order spacing in Pips", DefaultValue = 1)]
        public int OrderSpacing { get; set; }

        [Parameter("# Order placed before order spacing multiplies", DefaultValue = 10)]
        public int OrderSpacingLevels { get; set; }

        [Parameter("Order spacing multipler", DefaultValue = 2)]
        public double OrderSpacingMultipler { get; set; }

        [Parameter("Order spacing max", DefaultValue = 3)]
        public int OrderSpacingMax { get; set; }

        [Parameter("# of Limit Orders", DefaultValue = 40)]
        public int NumberOfOrders { get; set; }

        [Parameter("Volume (Lots)", DefaultValue = 1)]
        public int Volume { get; set; }

        [Parameter("Volume Max (Lots)", DefaultValue = 200)]
        public int VolumeMax { get; set; }

        [Parameter("# Order placed before Volume multiplies", DefaultValue = 5)]
        public int OrderVolumeLevels { get; set; }

        [Parameter("Volume multipler", DefaultValue = 2)]
        public double VolumeMultipler { get; set; }

        [Parameter("Take Profit", DefaultValue = 0.5)]
        public double DefaultTakeProfit { get; set; }

        [Parameter("Mins after swordfish period to reduce position risk", DefaultValue = 45)]
        public int ReduceRiskAfterMins { get; set; }

        [Parameter("Mins after swordfish period to reduce position risk", DefaultValue = 45)]
        public int CloseAfterMins { get; set; }

        [Parameter("Retrace level 1 Percentage", DefaultValue = 33)]
        public int RetraceLevel1 { get; set; }

        [Parameter("Retrace level 2 Percentage", DefaultValue = 50)]
        public int RetraceLevel2 { get; set; }

        [Parameter("Retrace level 3 Percentage", DefaultValue = 66)]
        public int RetraceLevel3 { get; set; }

        [Parameter("Initial Hard SL for last Order placed", DefaultValue = 5)]
        public double FinalOrderStopLoss { get; set; }

        [Parameter("Triggered Hard SL buffer", DefaultValue = 20)]
        public double HardStopLossBuffer { get; set; }

        [Parameter("Trailing SL fixed distance", DefaultValue = 5)]
        public double TrailingStopPips { get; set; }

        private State BotState;
        private FixedTrailingStop TrailingStop;
        private StopLossManager StopLossManager;
        private SpikeManager SpikeManager;
        private ProfitReporter ProfitReporter;
        private MarketInfo SwfMarketInfo;
        private SellLimitOrdersTrader SellLimitOrdersTrader;
        private BuyLimitOrdersTrader BuyLimitOrdersTrader;
        

        private RulesManager RulesManager;

        protected override void OnStart()
        {
            BotState = new State(this);
            SwfMarketInfo = BotState.getMarketInfo();
            SwfMarketInfo.SetCloseAfterMinutes(CloseAfterMins);
            SwfMarketInfo.SetReduceRiskAfterMinutes(ReduceRiskAfterMins);

            SellLimitOrdersTrader = new SellLimitOrdersTrader(BotState, NumberOfOrders, OrderEntryOffset, DefaultTakeProfit, FinalOrderStopLoss);
            BuyLimitOrdersTrader = new BuyLimitOrdersTrader(BotState, NumberOfOrders, OrderEntryOffset, DefaultTakeProfit, FinalOrderStopLoss);
            StopLossManager = new StopLossManager(BotState, HardStopLossBuffer, FinalOrderStopLoss);

            RulesManager = new RulesManager(BotState, BuyLimitOrdersTrader, SpikeManager, StopLossManager, new FixedTrailingStop(BotState, TrailingStopPips));

            Positions.Opened += PositionsOnOpened;
            Positions.Closed += PositionsOnClosed;
        }

            protected override void OnTick()
            {
                // If backtesting use the Server.Time.        
                if (SwfMarketInfo.IsBotTradingOpen())
                {
                    //Start Swordfishing
                    if (BotState.IsReset)
                        BotState.IsReset = false;

                    if (!BotState.OpenPriceCaptured)
                    {
                        //Get the Market Open Price
                        BotState.OpenPrice = MarketSeries.Close.LastValue;
                        BotState.OpenPriceCaptured = true;
                    }

                    if (!BotState.OrdersPlaced)
                    {
                    //Price moves TriggerOrderPlacementPips UP from open then look to set SELL LimitOrders
                    if (BotState.OpenPrice + TriggerOrderPlacementPips < Symbol.Bid)
                        {
                            SellLimitOrdersTrader.placeSellLimitOrders();
                        }
                        //Price moves 5pts DOWN from open then look to set BUY LimitOrders
                        else if (BotState.OpenPrice - TriggerOrderPlacementPips > Symbol.Ask)
                        {
                            BuyLimitOrdersTrader.placeBuyLimitOrders();
                        }
                    }

                    SpikeManager.captureSpike();
                }
                //It is outside SwordFish Time
                else
                {

                    //TO DO: NEED TO APPLY THE RULES 'ARE ORDERS ARE PLACED' and 'ARE ORDERS OPEN' TO ALL THE RULES THEY APPLIES TO
                    // DO NOT CHAIN RULES - EVERY RULE SHOULD BE ABLE TO EXECUTE IN ISOLATION
                    if (BotState.OrdersPlaced)
                    {
                        if (BotState.positionsRemainOpen())
                        {

                            //If positions were opened on the last tick at close time then may not have recorded a spike peak
                            if (!SpikeManager.isSpikeCaptured())
                                SpikeManager.captureSpike();

                            //Check the state based on the time from open
                            BotState.checkTimeState();

                            //ManagePositionRisk();
                            RulesManager.runAllRules();
                        }
                        else
                        {
                            //No positions opened and out of Swordfish time
                            if (BotState.IsPendingOrdersClosed)
                                BuyLimitOrdersTrader.closeAllPendingOrders();
                                ResetSwordFish();
                        }

                        //Out of Swordfish time and all positions that were opened are now closed
                        if (BotState.allPositionsClosed())
                            ResetSwordFish();
                    }
                    //No Orders were placed and it is out of swordfish time therefore reset Swordfish
                    else
                    {
                        ResetSwordFish();
                    }
                }

            //If the Trail is active then chase.
            TrailingStop.chase();
            }

            protected void PositionsOnOpened(PositionOpenedEventArgs args)
            {
                if (BotState.isThisBotId(args.Position.Label))
                {
                    BotState.OpenedPositionsCount++;

                //Capture last Position Opened i.e. the furthest away
                BotState.LastPositionTradeType = args.Position.TradeType;
                BotState.LastPositionEntryPrice = args.Position.EntryPrice;
                BotState.LastPositionLabel = args.Position.Label;
                }

            }

            protected void PositionsOnClosed(PositionClosedEventArgs args)
            {
                if (BotState.isThisBotId(args.Position.Label))
                {
                    BotState.ClosedPositionsCount++;

                    ProfitReporter.reportTrade(args.Position);
                   
                    //Last position's SL has been triggered for a loss - NOT a swordfish
                    if (BotState.LastPositionLabel == args.Position.Label && args.Position.GrossProfit < 0)
                    {
                        Print("CLOSING ALL POSITIONS due to furthest position losing");
                        BuyLimitOrdersTrader.closeAllPendingOrders();
                        PositionsManager.closeAllPositions();
                        BotState.IsTerminated = true;
                    }

                    //Taking profit
                    if (args.Position.GrossProfit > 0)
                    {
                        //capture last position take profit price
                        BotState.captureLastProfitPositionPrices(args.Position);

                    //If the spike has retraced then close all pending and set trailing stop
                    RulesManager.RunAll(); // ManagePositionRisk();

                        //BreakEven SL triggered in ManageRisk() function
                        if (BotState.IsBreakEvenStopLossActive)
                        {
                            StopLossManager.setBreakEvenSLForAllPositions(BotState.LastProfitPositionEntryPrice, true);
                        }
                    }
                }
            }

         
            protected override void OnStop()
            {
            // Put your deinitialization logic here

            }

        }

    }


    





















