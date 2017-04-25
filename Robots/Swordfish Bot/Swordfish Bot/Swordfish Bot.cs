using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FileSystem)]
    public class SwordfishBot : Robot
    {

        [Parameter("Source")]
        public DataSeries DataSeriesSource { get; set; }

        [Parameter("Use Bollinger Bollinger Band Entry", DefaultValue = false)]
        public bool useBollingerBandEntry { get; set; }

        [Parameter("Pips inside Bollinger Band Entry", DefaultValue = 2)]
        public int targetBolliEntryPips { get; set; }

        [Parameter("Initial Order placement trigger from open", DefaultValue = 5)]
        public int SwordFishTrigger { get; set; }

        [Parameter("Offset from Market Open for First Order", DefaultValue = 9)]
        public int OrderEntryOffset { get; set; }

        [Parameter("Distance between Orders in Pips", DefaultValue = 1)]
        public int OrderSpacing { get; set; }

        [Parameter("# of Limit Orders", DefaultValue = 40)]
        public int NumberOfOrders { get; set; }

        [Parameter("Volume (Lots)", DefaultValue = 1)]
        public int Volume { get; set; }

        [Parameter("Volume Max (Lots)", DefaultValue = 200)]
        public int VolumeMax { get; set; }

        [Parameter("# Order placed before Volume multiples", DefaultValue = 5)]
        public int OrderVolumeLevels { get; set; }

        [Parameter("Volume multipler", DefaultValue = 2)]
        public int VolumeMultipler { get; set; }

        [Parameter("Take Profit", DefaultValue = 0.5)]
        public double TakeProfit { get; set; }

        [Parameter("Mins after swordfish period to reduce position risk", DefaultValue = 45)]
        public int ReducePositionRiskTime { get; set; }

        [Parameter("Enable Retrace risk management", DefaultValue = true)]
        public bool retraceEnabled { get; set; }

        [Parameter("Retrace level 1 Percentage", DefaultValue = 33)]
        public int retraceLevel1 { get; set; }

        [Parameter("Retrace level 2 Percentage", DefaultValue = 50)]
        public int retraceLevel2 { get; set; }

        [Parameter("Retrace level 3 Percentage", DefaultValue = 66)]
        public int retraceLevel3 { get; set; }

        [Parameter("Initial Hard SL for last Order placed", DefaultValue = 5)]
        public double FinalOrderStopLoss { get; set; }

        [Parameter("Triggered Hard SL buffer", DefaultValue = 20)]
        public double HardStopLossBuffer { get; set; }

        [Parameter("Trailing SL fixed distance", DefaultValue = 5)]
        public double TrailingStopPips { get; set; }

        protected MarketTimeInfo swordFishTimeInfo;
        protected BollingerBands Boli;

        //Price and Position Variables
        protected double OpenPrice;
        protected string LastPositionLabel;
        protected TradeType LastPositionTradeType;
        protected double LastPositionEntryPrice;
        protected double LastClosedPositionEntryPrice;
        protected double LastProfitPrice;

        protected double OpenedPositionsCount = 0;
        protected double ClosedPositionsCount = 0;

        //Stop Loss Variables
        protected double DivideTrailingStopPips = 1;
        protected bool isTrailingStopsActive = false;
        protected bool isHardSLLastProfitPrice = false;
        protected bool isHardSLLastPositionEntryPrice = false;
        protected bool isHardSLLastClosedPositionEntryPrice = false;
        protected bool isBreakEvenStopLossActive = false;

        //Swordfish State Variables
        protected bool isPendingOrdersClosed = false;
        protected bool OpenPriceCaptured = false;
        protected bool OrdersPlaced = false;
        protected bool isSwordfishTerminated = false;
        protected bool isSwordFishReset = true;
        protected bool isReducedRiskTime = false;
        protected string botId = null;

        List<string> debugCSV = new List<string>();

        //Performance Reporting
        protected double DayProfitTotal = 0;
        protected double DayPipsTotal = 0;

        protected override void OnStart()
        {
            botId = generateBotId();
            swordFishTimeInfo = new MarketTimeInfo();
            setTimeZone();
            Boli = Indicators.BollingerBands(DataSeriesSource, 2, 20, MovingAverageType.Exponential);

            Positions.Opened += PositionsOnOpened;
            Positions.Closed += PositionsOnClosed;

            debugCSV.Add("Trade,Profit,Pips,Day,Label,EntryPrice,ClosePrice,SL,TP,Date/Time,OpenedPositionsCount,ClosedPositionsCount,LastPositionEntryPrice,LastClosedPositionEntryPrice,LastProfitPrice,LastPositionLabel,DivideTrailingStopPips,isTrailingStopsActive,isBreakEvenStopLossActive,isHardSLLastClosedPositionEntryPrice,isHardSLLastPositionEntryPrice,isHardSLLastProfitPrice,OpenPriceCaptured,OrdersPlaced,isSwordFishReset,isSwordfishTerminated,isReducedRiskTime");
        }

        protected string generateBotId()
        {
            Random randomIdGenerator = new Random();
            int id = randomIdGenerator.Next(0, 99999);
            return id.ToString("00000");
        }


        protected override void OnTick()
        {
            // If backtesting use the Server.Time.        
            if (IsSwordFishTime())
            {
                //Start Swordfishing
                if (isSwordFishReset)
                    isSwordFishReset = false;

                if (!OpenPriceCaptured)
                {
                    //Get the Market Open Price
                    OpenPrice = MarketSeries.Close.LastValue;
                    OpenPriceCaptured = true;
                }

                if (!OrdersPlaced)
                {
                    //Price moves 5pts UP from open then look to set SELL LimitOrders
                    if (OpenPrice + SwordFishTrigger < Symbol.Bid)
                    {
                        placeSellLimitOrders();
                    }
                    //Price moves 5pts DOWN from open then look to set BUY LimitOrders
                    else if (OpenPrice - SwordFishTrigger > Symbol.Ask)
                    {
                        placeBuyLimitOrders();
                    }
                }
            }
            //It is outside SwordFish Time
            else
            {
                if (OrdersPlaced)
                {
                    if (OpenedPositionsCount - ClosedPositionsCount > 0)
                    {
                        //Look to reduce risk as Spike retraces
                        ManagePositionRisk();

                        //Positions still open after ReducePositionRiskTime
                        if (!isReducedRiskTime && swordFishTimeInfo.IsReduceRiskTime(IsBacktesting, Server.Time, ReducePositionRiskTime))
                        {
                            //Reduce Trailing Stop Loss by 50%
                            // DivideTrailingStopPips = 2;
                            isReducedRiskTime = true;
                        }

                        //If trades still open at ClosingAllTime then take the hit and close remaining positions
                        if (!isSwordfishTerminated && swordFishTimeInfo.IsCloseAllPositionsTime(IsBacktesting, Server.Time))
                        {
                            CloseAllPositions();
                            isSwordfishTerminated = true;
                        }
                    }
                    else
                    {
                        //No positions opened and out of Swordfish time
                        if (!isPendingOrdersClosed)
                            CloseAllPendingOrders();
                        ResetSwordFish();
                    }

                    //Out of Swordfish time and all positions that were opened are now closed
                    if (OpenedPositionsCount > 0 && OpenedPositionsCount - ClosedPositionsCount == 0)
                        ResetSwordFish();
                }
                //No Orders were placed and it is out of swordfish time therefore reset Swordfish
                else
                {
                    ResetSwordFish();
                }
            }

            // If Trailing stop is active update position SL's
            if (isTrailingStopsActive)
            {
                List<Task> taskList = new List<Task>();
                foreach (Position p in Positions)
                {
                    taskList.Add(Task.Factory.StartNew((Object obj) =>
                    {
                        try
                        {
                            if (isThisBotId(p.Label))
                            {

                                double newStopLossPrice = calcTrailingStopLoss(p);
                                if (newStopLossPrice > 0)
                                {
                                    ModifyPositionAsync(p, newStopLossPrice, null, onTradeOperationComplete);
                                }
                            }
                        } catch (Exception e)
                        {
                            Print("Failed to Modify Position:" + e.Message);
                        }

                    }, p));
                }
                Task.WaitAll(taskList.ToArray<Task>());
            }
        }

        protected void setBreakEvens(double breakEvenTriggerPrice)
        {
            List<Task> taskList = new List<Task>();
            foreach (Position p in Positions)
            {
                taskList.Add(Task.Factory.StartNew((Object obj) =>
                {
                    try
                    {
                        if (isThisBotId(p.Label))
                        {
                            if (LastPositionTradeType == TradeType.Buy)
                            {
                                if (breakEvenTriggerPrice > p.EntryPrice)
                                {
                                    ModifyPositionAsync(p, p.EntryPrice + HardStopLossBuffer, p.TakeProfit, onTradeOperationComplete);
                                }
                            }

                            if (LastPositionTradeType == TradeType.Sell)
                            {
                                if (breakEvenTriggerPrice < p.EntryPrice)
                                {
                                    ModifyPositionAsync(p, p.EntryPrice - HardStopLossBuffer, p.TakeProfit, onTradeOperationComplete);
                                }
                            }
                        }
                    } catch (Exception e)
                    {
                        Print("Failed to Modify Position:" + e.Message);
                    }
                }, p));
            }
            Task.WaitAll(taskList.ToArray<Task>());
        }


        //calculate Trailing Stop Loss
        protected double calcTrailingStopLoss(Position position)
        {
            double newStopLossPips = 0;
            double newStopLossPrice = 0;
            double currentStopLossPips = 0;
            double currentStopLossPrice = 0;

            bool isProtected = position.StopLoss.HasValue;
            if (isProtected)
            {
                currentStopLossPrice = (double)position.StopLoss;
            }
            else
            {
                //Should never happen
                Print("WARNING: Trailing Stop Loss Activated but No intial STOP LESS set");
                currentStopLossPrice = LastPositionEntryPrice;
            }

            if (position.TradeType == TradeType.Buy)
            {
                newStopLossPrice = Symbol.Ask - TrailingStopPips / DivideTrailingStopPips;
                newStopLossPips = position.EntryPrice - newStopLossPrice;
                currentStopLossPips = position.EntryPrice - currentStopLossPrice;

                //Is newStopLoss more risk than current SL
                if (newStopLossPips < currentStopLossPips)
                    return 0;

                //Is newStopLoss more than the current Ask and therefore not valid
                if (newStopLossPrice > Symbol.Ask)
                    return 0;

                //Is the difference between the newStopLoss and the current SL less than the tick size and therefore not valid
                if (currentStopLossPips - newStopLossPips < Symbol.TickSize)
                    return 0;
            }

            if (position.TradeType == TradeType.Sell)
            {
                newStopLossPrice = Symbol.Bid + TrailingStopPips / DivideTrailingStopPips;
                newStopLossPips = newStopLossPrice - position.EntryPrice;
                currentStopLossPips = currentStopLossPrice - position.EntryPrice;

                //Is newStopLoss more risk than current SL
                if (newStopLossPips > currentStopLossPips)
                    return 0;

                //Is newStopLoss more than the current Ask and therefore not valid
                if (newStopLossPrice < Symbol.Bid)
                    return 0;

                //Is the difference between the newStopLoss and the current SL less than the tick size and therefore not valid
                if (currentStopLossPips - newStopLossPips < Symbol.TickSize)
                    return 0;
            }

            return newStopLossPrice;
        }

        //Place Buy Limit Orders
        protected void placeBuyLimitOrders()
        {
            //Place Buy Limit Orders
            List<Task> taskList = new List<Task>();
            for (int OrderCount = 0; OrderCount < NumberOfOrders; OrderCount++)
            {
                taskList.Add(Task.Factory.StartNew((Object obj) =>
                {
                    try
                    {
                        tradeData data = obj as tradeData;
                        if (data == null)
                            return;

                        //Check that entry price is valid
                        if (data.entryPrice < Symbol.Bid)
                        {
                            PlaceLimitOrderAsync(data.tradeType, data.symbol, data.volume, data.entryPrice, data.label, data.stopLossPips, data.takeProfitPips, onTradeOperationComplete);
                        }
                        else
                        {
                            //Tick price has 'jumped' - therefore avoid placing all PendingOrders by re-calculating the OrderCount to the equivelant entry point.
                            OrderCount = calculateNewOrderCount(OrderCount, Symbol.Bid);
                            ExecuteMarketOrderAsync(data.tradeType, data.symbol, data.volume, data.label + "X", data.stopLossPips, data.takeProfitPips, onTradeOperationComplete);
                        }
                    } catch (Exception e)
                    {
                        Print("Failed to place buy limit order: " + e.Message);
                    }
                }, new tradeData 
                {
                    tradeType = TradeType.Buy,
                    symbol = Symbol,
                    volume = setVolume(OrderCount, NumberOfOrders),
                    entryPrice = calcBuyEntryPrice(OrderCount),
                    label = botId + "-" + getTimeStamp() + swordFishTimeInfo.market + "-SWF#" + OrderCount,
                    stopLossPips = setPendingOrderStopLossPips(OrderCount, NumberOfOrders),
                    takeProfitPips = TakeProfit * (1 / Symbol.TickSize)
                }));
            }
            Task.WaitAll(taskList.ToArray<Task>());

            //All Buy Limit Orders have been placed
            OrdersPlaced = true;
        }

        protected double calcBuyEntryPrice(int orderCount)
        {
            //OPTIONAL - Bollinger band indicates whether market is oversold or over bought.
            if (useBollingerBandEntry)
            {
                //Use Bolinger Band limit as first order entry point.
                return Boli.Bottom.Last(0) + targetBolliEntryPips - orderCount * OrderSpacing;
            }
            else
            {
                return OpenPrice - OrderEntryOffset - orderCount * OrderSpacing;
            }
        }

        // Place Sell Limit Orders
        protected void placeSellLimitOrders()
        {
            //Place Sell Limit Orders
            List<Task> taskList = new List<Task>();
            for (int OrderCount = 0; OrderCount < NumberOfOrders; OrderCount++)
            {
                taskList.Add(Task.Factory.StartNew((Object obj) =>
                {
                    try
                    {
                        tradeData data = obj as tradeData;
                        if (data == null)
                            return;

                        //Check that entry price is valid
                        if (data.entryPrice > Symbol.Ask)
                        {
                            PlaceLimitOrderAsync(data.tradeType, data.symbol, data.volume, data.entryPrice, data.label, data.stopLossPips, data.takeProfitPips, onTradeOperationComplete);
                        }
                        else
                        {
                            //Tick price has 'jumped' - therefore avoid placing all PendingOrders by re-calculating the OrderCount to the equivelant entry point.
                            OrderCount = calculateNewOrderCount(OrderCount, Symbol.Ask);
                            ExecuteMarketOrderAsync(data.tradeType, data.symbol, data.volume, data.label + "X", data.stopLossPips, data.takeProfitPips, onTradeOperationComplete);
                        }
                    } catch (Exception e)
                    {
                        Print("Failed to place Sell Limit Order: " + e.Message);
                    }

                }, new tradeData 
                {
                    tradeType = TradeType.Sell,
                    symbol = Symbol,
                    volume = setVolume(OrderCount, NumberOfOrders),
                    entryPrice = calcSellEntryPrice(OrderCount),
                    label = botId + "-" + getTimeStamp() + swordFishTimeInfo.market + "-SWF#" + OrderCount,
                    stopLossPips = setPendingOrderStopLossPips(OrderCount, NumberOfOrders),
                    takeProfitPips = TakeProfit * (1 / Symbol.TickSize)
                }));
            }
            Task.WaitAll(taskList.ToArray<Task>());

            //All Sell Limit Orders have been placed
            OrdersPlaced = true;
        }


        protected void onTradeOperationComplete(TradeResult tr)
        {
            if (!tr.IsSuccessful)
            {
                string msg = "FAILED Trade Operation: " + tr.Error;
                if (tr.Position != null)
                    Print(msg, " Position: ", tr.Position.Label, " ", tr.Position.TradeType, " ", Time);
                if (tr.PendingOrder != null)
                    Print(msg, " Pending Order: ", tr.PendingOrder.Label, " ", tr.PendingOrder.TradeType, " ", Time);
            }
        }

        protected double calcSellEntryPrice(int orderCount)
        {

            //OPTIONAL - Bollinger band indicates whether market is oversold or over bought.
            if (useBollingerBandEntry)
            {
                //Use Bolinger Band limit as first order entry point.
                return Boli.Top.Last(0) - targetBolliEntryPips + orderCount * OrderSpacing;
            }
            else
            {
                return OpenPrice + OrderEntryOffset + orderCount * OrderSpacing;
            }
        }


        //Calculate a new orderCount number for when tick jumps
        protected int calculateNewOrderCount(int _orderCount, double _currentTickPrice)
        {
            double tickJumpIntoRange = Math.Abs(OpenPrice - _currentTickPrice) - OrderEntryOffset;
            double pendingOrderRange = NumberOfOrders * OrderSpacing;
            double pendingOrdersPercentageJumped = tickJumpIntoRange / pendingOrderRange;
            double _newOrderCount = NumberOfOrders * pendingOrdersPercentageJumped;

            if (_newOrderCount > _orderCount)
                return (int)_newOrderCount;
            else
                return (int)_orderCount;
        }

        protected void PositionsOnOpened(PositionOpenedEventArgs args)
        {
            if (isThisBotId(args.Position.Label))
            {
                OpenedPositionsCount++;

                //Capture last Position Opened i.e. the furthest away
                LastPositionTradeType = args.Position.TradeType;
                LastPositionEntryPrice = args.Position.EntryPrice;
                LastPositionLabel = args.Position.Label;
            }

        }

        protected void PositionsOnClosed(PositionClosedEventArgs args)
        {
            if (isThisBotId(args.Position.Label))
            {
                ClosedPositionsCount++;

                DayProfitTotal += args.Position.GrossProfit;
                DayPipsTotal += args.Position.Pips;
                debugCSV.Add("TRADE," + args.Position.GrossProfit + "," + args.Position.Pips + "," + Time.DayOfWeek + "," + args.Position.Label + "," + args.Position.EntryPrice + "," + History.FindLast(args.Position.Label, Symbol, args.Position.TradeType).ClosingPrice + "," + args.Position.StopLoss + "," + args.Position.TakeProfit + "," + Time + debugState());

                //Last position's SL has been triggered for a loss - NOT a swordfish
                if (LastPositionLabel == args.Position.Label && args.Position.GrossProfit < 0)
                {
                    Print("CLOSING ALL POSITIONS due to furthest position losing");
                    CloseAllPendingOrders();
                    CloseAllPositions();
                    isSwordfishTerminated = true;
                }

                //Taking profit
                if (args.Position.GrossProfit > 0)
                {
                    //capture last position take profit price
                    setLastProfitPrice(args.Position.TradeType);

                    //capture last closed position entry price
                    LastClosedPositionEntryPrice = args.Position.EntryPrice;

                    //If the spike has retraced then close all pending and set trailing stop
                    ManagePositionRisk();

                    //BreakEven SL triggered in ManageRisk() function
                    if (isBreakEvenStopLossActive)
                    {
                        setBreakEvens(LastProfitPrice);
                    }
                }
            }
        }

        protected void ManagePositionRisk()
        {

            //Close any positions that have not been triggered
            if (!isPendingOrdersClosed)
                CloseAllPendingOrders();

            if (retraceEnabled)
            {
                //Calculate spike retrace factor
                double retraceFactor = calculateRetraceFactor();

                if (isReducedRiskTime)
                {
                    //reset HARD SL Limits with reduced SL's
                    isHardSLLastPositionEntryPrice = true;

                    //Reduce all retrace limits
                    retraceLevel1 = retraceLevel1 / 2;
                    retraceLevel2 = retraceLevel2 / 2;
                    retraceLevel3 = retraceLevel3 / 2;
                }

                //Set hard stop losses as soon as Swordfish time is over
                if (!isHardSLLastPositionEntryPrice && !IsSwordFishTime())
                {
                    setAllStopLosses(LastPositionEntryPrice);
                    isHardSLLastPositionEntryPrice = true;
                }

                //Set hard stop losses and activate Trail if Spike has retraced between than retraceLevel1 and retraceLevel2
                if (isReducedRiskTime || (retraceLevel2 > retraceFactor && retraceFactor > retraceLevel1))
                {
                    //If Hard SL has not been set yet
                    if (!isHardSLLastPositionEntryPrice && LastPositionEntryPrice > 0)
                    {
                        setAllStopLosses(LastPositionEntryPrice);
                        isHardSLLastPositionEntryPrice = true;
                    }
                    //Active Breakeven Stop Losses
                    isBreakEvenStopLossActive = true;
                }

                //Set harder SL and active BreakEven if it has retraced between than retraceLevel2 and retraceLevel3
                if (isReducedRiskTime || (retraceLevel3 > retraceFactor && retraceFactor > retraceLevel2))
                {
                    //Set hard stop losses
                    if (!isHardSLLastClosedPositionEntryPrice && LastClosedPositionEntryPrice > 0)
                    {
                        setAllStopLosses(LastClosedPositionEntryPrice);
                        isHardSLLastClosedPositionEntryPrice = true;
                    }
                    //Activate Trailing Stop Losses
                    isTrailingStopsActive = true;
                }

                //Set hardest SL if Spike retraced past retraceLevel3
                if (isReducedRiskTime || retraceFactor > retraceLevel3)
                {
                    //Set hard stop losses
                    if (!isHardSLLastProfitPrice && LastProfitPrice > 0)
                    {
                        setAllStopLosses(LastProfitPrice);
                        isHardSLLastProfitPrice = true;
                    }
                }
            }
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
            if (LastPositionTradeType == TradeType.Sell)
            {
                //Position are Selling
                percentRetrace = (Symbol.Bid - OpenPrice) / (LastPositionEntryPrice - OpenPrice);
            }

            if (LastPositionTradeType == TradeType.Buy)
            {
                //Positions are buying
                percentRetrace = (OpenPrice - Symbol.Ask) / (OpenPrice - LastPositionEntryPrice);
            }

            percentRetrace = 1 - percentRetrace;
            percentRetrace = percentRetrace * 100;

            return percentRetrace;
        }

        protected double calculatePercentageClosed()
        {
            double percentClosed = 0;
            if (OpenedPositionsCount > 0)
            {
                percentClosed = (ClosedPositionsCount / OpenedPositionsCount) * 100;
            }
            return percentClosed;
        }


        protected void setLastProfitPrice(TradeType lastProfitTradeType)
        {
            if (lastProfitTradeType == TradeType.Buy)
                LastProfitPrice = Symbol.Ask;
            if (lastProfitTradeType == TradeType.Sell)
                LastProfitPrice = Symbol.Bid;
        }

        protected bool IsSwordFishTime()
        {
            return swordFishTimeInfo.IsPlacePendingOrdersTime(IsBacktesting, Server.Time);
        }

        protected void setAllStopLosses(double SLPrice)
        {
            switch (LastPositionTradeType)
            {
                case TradeType.Buy:
                    setStopLossForAllPositions(SLPrice - HardStopLossBuffer);
                    break;
                case TradeType.Sell:
                    setStopLossForAllPositions(SLPrice + HardStopLossBuffer);
                    break;
            }
        }

        //Set a stop loss on the last Pending Order set to catch the break away train that never comes back!
        protected double setPendingOrderStopLossPips(int _orderCount, int _numberOfOrders)
        {
            if (_orderCount == _numberOfOrders - 1)
            {
                return FinalOrderStopLoss * (1 / Symbol.TickSize);
            }
            else
            {
                return 0;
            }
        }

        //Increase the volume based on Orders places and volume levels and multiplier until max volume reached
        protected int setVolume(int _orderCount, int _numberOfOrders)
        {

            double _orderVolumeLevel = _orderCount / OrderVolumeLevels;
            double _volume = Math.Pow(VolumeMultipler, _orderVolumeLevel) * Volume;

            if (_volume > VolumeMax)
            {
                _volume = VolumeMax;
            }

            return (int)_volume;
        }

        protected void CloseAllPendingOrders()
        {
            //Close any outstanding pending orders
            List<Task> taskList = new List<Task>();
            foreach (PendingOrder po in PendingOrders)
            {
                taskList.Add(Task.Factory.StartNew((Object obj) =>
                {
                    try
                    {
                        if (isThisBotId(po.Label))
                        {
                            CancelPendingOrderAsync(po, onTradeOperationComplete);
                        }
                    } catch (Exception e)
                    {
                        Print("Failed to Cancel Pending Order :" + e.Message);
                    }
                }, po));
            }
            Task.WaitAll(taskList.ToArray<Task>());
            isPendingOrdersClosed = true;
        }

        protected void CloseAllPositions()
        {
            //Close any outstanding pending orders
            List<Task> taskList = new List<Task>();
            foreach (Position p in Positions)
            {
                taskList.Add(Task.Factory.StartNew((Object obj) =>
                {
                    try
                    {
                        if (isThisBotId(p.Label))
                        {
                            ClosePositionAsync(p, onTradeOperationComplete);
                        }
                    } catch (Exception e)
                    {
                        Print("Failed to Close Position: " + e.Message);
                    }
                }, p));
            }
            Task.WaitAll(taskList.ToArray<Task>());
        }

        protected void setStopLossForAllPositions(double _stopLossPrice)
        {
            List<Task> taskList = new List<Task>();
            foreach (Position p in Positions)
            {
                taskList.Add(Task.Factory.StartNew((Object obj) =>
                {
                    try
                    {
                        if (isThisBotId(p.Label))
                        {
                            ModifyPositionAsync(p, _stopLossPrice, p.TakeProfit, onTradeOperationComplete);
                        }
                    } catch (Exception e)
                    {
                        Print("Failed to Modify Position: " + e.Message);
                    }
                }, p));
            }
            Task.WaitAll(taskList.ToArray<Task>());
        }


        //Check whether a position or order is managed by this bot instance.
        protected bool isThisBotId(string label)
        {
            string id = label.Substring(0, 5);
            if (id.Equals(botId))
                return true;
            else
                return false;
        }

        protected void ResetSwordFish()
        {
            if (isSwordFishReset)
                return;

            reportDay();

            //reset position counters
            OpenedPositionsCount = 0;
            ClosedPositionsCount = 0;

            //reset Last Position variables
            LastPositionLabel = "NO LAST POSITION SET";
            LastPositionEntryPrice = 0;
            LastClosedPositionEntryPrice = 0;
            LastProfitPrice = 0;

            //reset risk management variables
            DivideTrailingStopPips = 1;
            isTrailingStopsActive = false;
            isBreakEvenStopLossActive = false;
            isHardSLLastClosedPositionEntryPrice = false;
            isHardSLLastPositionEntryPrice = false;
            isHardSLLastProfitPrice = false;

            // swordfish bot state variables
            OpenPriceCaptured = false;
            OrdersPlaced = false;
            isPendingOrdersClosed = false;
            isSwordfishTerminated = false;
            isSwordFishReset = true;
            isReducedRiskTime = false;

            // reset reporting variables
            DayProfitTotal = 0;
            DayPipsTotal = 0;
        }

        protected void reportDay()
        {
            string profit = "";
            if (DayProfitTotal != 0 && DayPipsTotal != 0)
            {
                profit = ("DAY TOTAL," + DayProfitTotal + "," + DayPipsTotal + "," + OpenedPositionsCount + "," + Time.DayOfWeek + "," + Time);
                debugCSV.Add(profit);
            }
        }



        protected string debugState()
        {

            string state = "";
            // Position counters
            state += "," + OpenedPositionsCount;
            state += "," + ClosedPositionsCount;

            // Last Position variables
            state += "," + LastPositionEntryPrice;
            state += "," + LastClosedPositionEntryPrice;
            state += "," + LastProfitPrice;
            state += "," + LastPositionLabel;

            // risk management variables
            state += "," + DivideTrailingStopPips;
            state += "," + isTrailingStopsActive;
            state += "," + isBreakEvenStopLossActive;
            state += "," + isHardSLLastClosedPositionEntryPrice;
            state += "," + isHardSLLastPositionEntryPrice;
            state += "," + isHardSLLastProfitPrice;

            // swordfish bot state variables
            state += "," + isHardSLLastProfitPrice;
            state += "," + OrdersPlaced;
            state += "," + isSwordFishReset;
            state += "," + isSwordfishTerminated;
            state += "," + isReducedRiskTime;

            return state;
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
            System.IO.File.WriteAllLines("C:\\Users\\alist\\Desktop\\swordfish\\" + swordFishTimeInfo.market + "-" + botId + "-" + "swordfish-" + getTimeStamp(true) + ".csv", debugCSV.ToArray());
        }

        protected string getTimeStamp(bool unformatted = false)
        {
            if (unformatted)
                return Time.Year.ToString() + Time.Month + Time.Day + Time.Minute + Time.Second;
            return Time.Year + "-" + Time.Month + "-" + Time.Day;
        }

        protected void setTimeZone()
        {

            switch (Symbol.Code)
            {
                case "UK100":
                    // Instantiate a MarketTimeInfo object.
                    swordFishTimeInfo.market = "FTSE";
                    swordFishTimeInfo.tz = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
                    // Market for swordfish trades opens at 8:00am.
                    swordFishTimeInfo.open = new TimeSpan(8, 0, 0);
                    // Market for swordfish trades closes at 8:05am.
                    swordFishTimeInfo.close = new TimeSpan(8, 5, 0);
                    // Close all open Swordfish position at 11:29am before US opens.
                    swordFishTimeInfo.closeAll = new TimeSpan(11, 29, 0);

                    break;
                case "GER30":
                    swordFishTimeInfo.market = "DAX";
                    swordFishTimeInfo.tz = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                    // Market for swordfish opens at 9:00.
                    swordFishTimeInfo.open = new TimeSpan(9, 0, 0);
                    // Market for swordfish closes at 9:05.
                    swordFishTimeInfo.close = new TimeSpan(9, 5, 0);
                    // Close all open Swordfish position at 11:29am before US opens.
                    swordFishTimeInfo.closeAll = new TimeSpan(11, 29, 0);
                    break;
                case "HK50":
                    swordFishTimeInfo.market = "HSI";
                    swordFishTimeInfo.tz = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
                    // Market for swordfish opens at 9:00.
                    swordFishTimeInfo.open = new TimeSpan(9, 30, 0);
                    // Market for swordfish closes at 9:05.
                    swordFishTimeInfo.close = new TimeSpan(9, 35, 0);
                    // Close all open Swordfish positions
                    swordFishTimeInfo.closeAll = new TimeSpan(11, 30, 0);
                    break;
            }
        }


    }

}


//Manage Market Opening Times
public struct MarketTimeInfo
{
    public String market;
    public TimeZoneInfo tz;
    public TimeSpan open;
    public TimeSpan close;
    public TimeSpan closeAll;

    //Is the current time within the period Swordfish Pending Orders can be placed
    public bool IsPlacePendingOrdersTime(bool _isBackTesting, DateTime _serverTime)
    {
        if (_isBackTesting)
        {
            return IsOpenAt(_serverTime);
        }
        else
        {
            return IsOpenAt(DateTime.UtcNow);
        }
    }

    //Time during which Swordfish positions risk should be managed
    public bool IsReduceRiskTime(bool _isBackTesting, DateTime _serverTime, int reduceRiskTimeFromOpen)
    {
        if (_isBackTesting)
        {
            return IsReduceRiskAt(_serverTime, reduceRiskTimeFromOpen);
        }
        else
        {
            return IsReduceRiskAt(DateTime.UtcNow, reduceRiskTimeFromOpen);
        }
    }

    //Is the current time within the period Swordfish positions can remain open.
    public bool IsCloseAllPositionsTime(bool _isBackTesting, DateTime _serverTime)
    {

        if (_isBackTesting)
        {
            return IsCloseAllAt(_serverTime);
        }
        else
        {
            return IsCloseAllAt(DateTime.UtcNow);
        }
    }

    //Is the current time within the period Swordfish Pending Orders can be placed.
    public bool IsOpenAt(DateTime _dateTimeUtc)
    {
        DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(_dateTimeUtc, tz);
        return (tzTime.TimeOfDay >= open & tzTime.TimeOfDay <= close);
    }

    //Is the current time after the time period when risk should be reduced.
    public bool IsReduceRiskAt(DateTime _dateTimeUtc, int reduceRiskTimeFromOpen)
    {
        DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(_dateTimeUtc, tz);
        return (tzTime.TimeOfDay >= open.Add(TimeSpan.FromMinutes(reduceRiskTimeFromOpen)));
    }

    //Is the current time within the period Swordfish positions can remain open.
    public bool IsCloseAllAt(DateTime _dateTimeUtc)
    {
        DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(_dateTimeUtc, tz);
        return tzTime.TimeOfDay >= closeAll;
    }
}

class tradeData
{
    public TradeType tradeType;
    public Symbol symbol;
    public int volume;
    public double entryPrice;
    public string label;
    public double stopLossPips;
    public double takeProfitPips;
}









