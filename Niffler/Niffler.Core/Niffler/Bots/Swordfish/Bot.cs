using System;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using Niffler.Common;
using Niffler.Common.TrailingStop;
using Niffler.Common.Market;

namespace Niffler.Bots.Swordfish

{
    public class Bot : cAlgo.API.Robot
    { 
            [Parameter("Source")]
            public DataSeries DataSeriesSource { get; set; }

            [Parameter("Use Bollinger Bollinger Band Entry", DefaultValue = false)]
            public bool UseBollingerBandEntry { get; set; }

            [Parameter("Pips inside Bollinger Band Entry", DefaultValue = 2)]
            public int TargetBolliEntryPips { get; set; }

            [Parameter("Initial Order placement trigger from open", DefaultValue = 5)]
            public int SwordFishTrigger { get; set; }

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
            public double TakeProfit { get; set; }

            [Parameter("Mins after swordfish period to reduce position risk", DefaultValue = 45)]
            public int ReducePositionRiskTime { get; set; }

            [Parameter("Enable Retrace risk management", DefaultValue = true)]
            public bool retraceEnabled { get; set; }

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


            private BollingerBands _boli;

            private State BotState;
            private FixedTrailingStop TrailingStop;
            private StopLossManager StopLossManager;
            private SpikeManager SpikeManager;

            protected override void OnStart()
            {
                BotState = new State();
                TrailingStop = new FixedTrailingStop(this, BotState, TrailingStopPips);
                StopLossManager = new StopLossManager(this, BotState, HardStopLossBuffer);
                SpikeManager = new SpikeManager(this,BotState);

                Debug Debug = new Debug();
                Debug.writeHeaders();


                _swordFishTimeInfo = new MarketTimeInfo();
                setTimeZone();
                _boli = Indicators.BollingerBands(DataSeriesSource, 2, 20, MovingAverageType.Exponential);

                Positions.Opened += PositionsOnOpened;
                Positions.Closed += PositionsOnClosed;
            }

            protected override void OnTick()
            {
                // If backtesting use the Server.Time.        
                if (IsSwordFishTime())
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
                        //Price moves 5pts UP from open then look to set SELL LimitOrders
                        if (BotState.OpenPrice + SwordFishTrigger < Symbol.Bid)
                        {
                            placeSellLimitOrders();
                        }
                        //Price moves 5pts DOWN from open then look to set BUY LimitOrders
                        else if (BotState.OpenPrice - SwordFishTrigger > Symbol.Ask)
                        {
                            placeBuyLimitOrders();
                        }
                    }

                    SpikeManager.captureSpike();
                }
                //It is outside SwordFish Time
                else
                {
                    if (BotState.OrdersPlaced)
                    {
                        if (BotState.positionsRemainOpen())
                        {

                            //If positions were opened at the end of swordfish time but have not recorded a spike peak
                            if (!SpikeManager.SpikePeakCaptured)
                                SpikeManager.captureSpike();

                            //Look to reduce risk as Spike retraces
                            RulesManager.runAll() //ManagePositionRisk();

                            //Positions still open after ReducePositionRiskTime
                            if (!BotState.IsReducedRiskTime && _swordFishTimeInfo.IsReduceRiskTime(IsBacktesting, Server.Time, ReducePositionRiskTime))
                            {
                                //Reduce Trailing Stop Loss by 50%
                                // DivideTrailingStopPips = 2;
                                BotState.IsReducedRiskTime = true;
                            }

                            //If trades still open at ClosingAllTime then take the hit and close remaining positions
                            if (!BotState.IsTerminated && _swordFishTimeInfo.IsCloseAllPositionsTime(IsBacktesting, Server.Time))
                            {
                                CloseAllPositions();
                                BotState.IsTerminated = true;
                            }
                        }
                        else
                        {
                            //No positions opened and out of Swordfish time
                            if (BotState.IsPendingOrdersClosed)
                                CloseAllPendingOrders();
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

                    _dayProfitTotal += args.Position.GrossProfit;
                    _dayPipsTotal += args.Position.Pips;

                    Debug.writeTrade();
                   
                    //Last position's SL has been triggered for a loss - NOT a swordfish
                    if (BotState.LastPositionLabel == args.Position.Label && args.Position.GrossProfit < 0)
                    {
                        Print("CLOSING ALL POSITIONS due to furthest position losing");
                        CloseAllPendingOrders();
                        CloseAllPositions();
                        BotState.IsTerminated = true;
                    }

                    //Taking profit
                    if (args.Position.GrossProfit > 0)
                    {
                        //capture last position take profit price
                        setLastProfitPrice(args.Position.TradeType);

                    //capture last closed position entry price
                    BotState.LastClosedPositionEntryPrice = args.Position.EntryPrice;

                        //If the spike has retraced then close all pending and set trailing stop
                        ManagePositionRisk();

                        //BreakEven SL triggered in ManageRisk() function
                        if (BotState.IsBreakEvenStopLossActive)
                        {
                            BreakEvenStop.chase(BotState.LastProfitPrice);
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


    





















