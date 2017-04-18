using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using System.Collections.Generic;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FileSystem)]
    public class SwordfishBot : Robot
    {

        [Parameter("Source")]
        public DataSeries DataSeriesSource { get; set; }

        [Parameter("Check Bollinger Bollinger Band", DefaultValue = false)]
        public bool checkBollingerBand { get; set; }

        [Parameter("Pips inside Bollinger Band", DefaultValue = 2)]
        public int targetEntryPips { get; set; }

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

        List<string> debugCSV = new List<string>();

        //Performance Reporting
        protected double DayProfitTotal = 0;
        protected double DayPipsTotal = 0;
        protected override void OnStart()
        {
            swordFishTimeInfo = new MarketTimeInfo();
            setTimeZone();
            Boli = Indicators.BollingerBands(DataSeriesSource, 2, 20, MovingAverageType.Exponential);

            Positions.Opened += PositionsOnOpened;
            Positions.Closed += PositionsOnClosed;

            debugCSV.Add("Trade,Profit,Pips,Day,Label,EntryPrice,ClosePrice,SL,TP,Date/Time,OpenedPositionsCount,ClosedPositionsCount,LastPositionEntryPrice,LastClosedPositionEntryPrice,LastProfitPrice,LastPositionLabel,DivideTrailingStopPips,isTrailingStopsActive,isBreakEvenStopLossActive,isHardSLLastClosedPositionEntryPrice,isHardSLLastPositionEntryPrice,isHardSLLastProfitPrice,OpenPriceCaptured,OrdersPlaced,isSwordFishReset,isSwordfishTerminated,isReducedRiskTime");
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
                        //Place Sell Limit Orders
                        for (int OrderCount = 0; OrderCount < NumberOfOrders; OrderCount++)
                        {
                            //OPTIONAL - Confirm last bar broke the Bollinger Band Top indicating market is overbought
                            if (checkBollingerBand)
                            {
                                if (Boli.Top.Last(0) < MarketSeries.Close.LastValue)
                                {
                                    PlaceLimitOrder(TradeType.Sell, Symbol, setVolume(OrderCount, NumberOfOrders), (OpenPrice + OrderEntryOffset + OrderCount * OrderSpacing), "SWORDFISH#" + OrderCount + "-" + getTimeStamp(), setPendingOrderStopLossPips(OrderCount, NumberOfOrders), TakeProfit * (1 / Symbol.TickSize));
                                }
                            }
                            else
                            {
                                //Check that entry price is valid
                                double EntryPrice = OpenPrice + OrderEntryOffset + OrderCount * OrderSpacing;
                                if (EntryPrice > Symbol.Ask)
                                {
                                    TradeResult SellLimitOrder = PlaceLimitOrder(TradeType.Sell, Symbol, setVolume(OrderCount, NumberOfOrders), EntryPrice, "SWORDFISH#" + OrderCount + "-" + getTimeStamp(), setPendingOrderStopLossPips(OrderCount, NumberOfOrders), TakeProfit * (1 / Symbol.TickSize));
                                    if (!SellLimitOrder.IsSuccessful)
                                        debug("FAILED to place order", SellLimitOrder);
                                }
                                else
                                {
                                    //Avoid placing all PendingOrders that have been 'jumped' by re-calculating the OrderCount to the equivelant entry point.
                                    //OrderCount = calculateNewOrderCount(OrderCount, Symbol.Ask);
                                    TradeResult SellOrder = ExecuteMarketOrder(TradeType.Sell, Symbol, setVolume(OrderCount, NumberOfOrders), "SWORDFISH-X#" + OrderCount + "-" + getTimeStamp(), setPendingOrderStopLossPips(OrderCount, NumberOfOrders), TakeProfit * (1 / Symbol.TickSize));
                                    if (!SellOrder.IsSuccessful)
                                        debug("FAILED to place order", SellOrder);
                                }
                            }
                        }
                        //All Sell Stop Orders have been placed
                        OrdersPlaced = true;
                    }
                    //Price moves 5pts DOWN from open then look to set BUY LimitOrders
                    else if (OpenPrice - SwordFishTrigger > Symbol.Ask)
                    {
                        //Place Buy Limit Orders
                        for (int OrderCount = 0; OrderCount < NumberOfOrders; OrderCount++)
                        {
                            //confirm last bar broke the Bollinger Band Top indicating overbought - OPTIONAL
                            if (checkBollingerBand)
                            {
                                //OPTIONAL - Confirm last bar broke the Bollinger Band Bottom indicating market is oversold
                                if (Boli.Bottom.Last(0) < MarketSeries.Close.LastValue)
                                {
                                    // Place BUY Limit order spaced by OrderSpacing 
                                    PlaceLimitOrder(TradeType.Buy, Symbol, setVolume(OrderCount, NumberOfOrders), (OpenPrice + OrderEntryOffset + OrderCount * OrderSpacing), "SWORDFISH#" + OrderCount + "-" + getTimeStamp(), setPendingOrderStopLossPips(OrderCount, NumberOfOrders), TakeProfit * (1 / Symbol.TickSize));
                                }

                            }
                            else
                            {
                                //Check that entry price is valid
                                double EntryPrice = OpenPrice - OrderEntryOffset - OrderCount * OrderSpacing;
                                if (EntryPrice < Symbol.Bid)
                                {
                                    TradeResult BuyLimitOrder = PlaceLimitOrder(TradeType.Buy, Symbol, setVolume(OrderCount, NumberOfOrders), EntryPrice, "SWORDFISH#" + OrderCount + "-" + getTimeStamp(), setPendingOrderStopLossPips(OrderCount, NumberOfOrders), TakeProfit * (1 / Symbol.TickSize));
                                    if (!BuyLimitOrder.IsSuccessful)
                                        debug("FAILED to place order", BuyLimitOrder);
                                }
                                else
                                {
                                    //Avoid placing all PendingOrders that have been 'jumped' by re-calculating the OrderCount to the equivelant entry point.
                                    OrderCount = calculateNewOrderCount(OrderCount, Symbol.Bid);
                                    TradeResult BuyOrder = ExecuteMarketOrder(TradeType.Buy, Symbol, setVolume(OrderCount, NumberOfOrders), "SWORDFISH-X#" + OrderCount + "-" + getTimeStamp(), setPendingOrderStopLossPips(OrderCount, NumberOfOrders), TakeProfit * (1 / Symbol.TickSize));
                                    if (!BuyOrder.IsSuccessful)
                                        debug("FAILED to place order", BuyOrder);
                                }
                            }
                        }
                        //All Buy Stop Orders have been placed
                        OrdersPlaced = true;
                    }
                }
            }
            //It is outside SwordFish Time
            else
            {
                if (OrdersPlaced)
                {
                    if (Positions.Count > 0)
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

                    //Out of Swordfish time and all positions that opened are now closed
                    if (OpenedPositionsCount > 0 && OpenedPositionsCount == ClosedPositionsCount)
                        ResetSwordFish();
                }
                //No Orders placed therefore reset Swordfish
                else
                {
                    ResetSwordFish();
                }
            }

            if (isTrailingStopsActive)
            {
                double newStopLossPips = 0;
                double newStopLossPrice = 0;
                double currentStopLossPips = 0;
                double currentStopLossPrice = 0;

                foreach (Position _p in Positions)
                {

                    bool isProtected = _p.StopLoss.HasValue;
                    if (isProtected)
                    {
                        currentStopLossPrice = (double)_p.StopLoss;
                    }
                    else
                    {
                        //Should never happen
                        Print("WARNING: Trail Activated but No intial STOP LESS set");
                        currentStopLossPrice = LastPositionEntryPrice;
                    }

                    if (_p.TradeType == TradeType.Buy)
                    {
                        newStopLossPrice = Symbol.Ask - TrailingStopPips / DivideTrailingStopPips;
                        newStopLossPips = _p.EntryPrice - newStopLossPrice;
                        currentStopLossPips = _p.EntryPrice - currentStopLossPrice;

                        //Is newStopLoss more risk than current SL
                        if (newStopLossPips < currentStopLossPips)
                            continue;

                        //Is newStopLoss more than the current Ask and therefore not valid
                        if (newStopLossPrice > Symbol.Ask)
                            continue;

                        //Is the difference between the newStopLoss and the current SL less than the tick size and therefore not valid
                        if (currentStopLossPips - newStopLossPips < Symbol.TickSize)
                            continue;
                    }

                    if (_p.TradeType == TradeType.Sell)
                    {
                        newStopLossPrice = Symbol.Bid + TrailingStopPips / DivideTrailingStopPips;
                        newStopLossPips = newStopLossPrice - _p.EntryPrice;
                        currentStopLossPips = currentStopLossPrice - _p.EntryPrice;

                        //Is newStopLoss more risk than current SL
                        if (newStopLossPips > currentStopLossPips)
                            continue;

                        //Is newStopLoss more than the current Ask and therefore not valid
                        if (newStopLossPrice < Symbol.Bid)
                            continue;

                        //Is the difference between the newStopLoss and the current SL less than the tick size and therefore not valid
                        if (currentStopLossPips - newStopLossPips < Symbol.TickSize)
                            continue;
                    }

                    TradeResult tr = ModifyPosition(_p, newStopLossPrice, _p.TakeProfit);
                    if (!tr.IsSuccessful)
                        debug("FAILED to modify SL", tr);

                }
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
            OpenedPositionsCount++;

            //Capture last Position Opened i.e. the furthest away
            LastPositionTradeType = args.Position.TradeType;
            LastPositionEntryPrice = args.Position.EntryPrice;
            LastPositionLabel = args.Position.Label;
        }

        protected void PositionsOnClosed(PositionClosedEventArgs args)
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
                    retraceLevel1 = retraceLevel1/2;
                    retraceLevel2 = retraceLevel2/2;
                    retraceLevel3 = retraceLevel3/2;
                }

                //Set hard stop losses as soon as Swordfish time is over
                if (!isHardSLLastPositionEntryPrice && !IsSwordFishTime())
                {
                    SetAllStopLosses(LastPositionEntryPrice);
                    isHardSLLastPositionEntryPrice = true;
                }

                //Set hard stop losses and activate Trail if Spike has retraced between than retraceLevel1 and retraceLevel2
                if (isReducedRiskTime || (retraceLevel2 > retraceFactor && retraceFactor > retraceLevel1))
                {
                    //If Hard SL has not been set yet
                    if (!isHardSLLastPositionEntryPrice && LastPositionEntryPrice > 0)
                    {
                        SetAllStopLosses(LastPositionEntryPrice);
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
                        SetAllStopLosses(LastClosedPositionEntryPrice);
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
                        SetAllStopLosses(LastProfitPrice);
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
                percentRetrace = (OpenPrice - Symbol.Bid) / (OpenPrice - LastPositionEntryPrice);
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


        protected void setBreakEvens(double breakEvenTriggerPrice)
        {
            TradeResult tr;
            foreach (Position _p in Positions)
            {
                if (LastPositionTradeType == TradeType.Buy)
                {
                    if (breakEvenTriggerPrice > _p.EntryPrice)
                    {
                        tr = ModifyPosition(_p, _p.EntryPrice + HardStopLossBuffer, _p.TakeProfit);
                        if (!tr.IsSuccessful)
                            debug("FAILED to modify", tr);
                    }

                }
                if (LastPositionTradeType == TradeType.Sell)
                {
                    if (breakEvenTriggerPrice < _p.EntryPrice)
                    {
                        tr = ModifyPosition(_p, _p.EntryPrice - HardStopLossBuffer, _p.TakeProfit);
                        if (!tr.IsSuccessful)
                            debug("FAILED to modify", tr);
                    }
                }
            }
        }

        protected void CloseAllPositions()
        {
            while (Positions.Count > 0)
            {
                TradeResult tr = ClosePosition(Positions[0]);
                if (!tr.IsSuccessful)
                    debug("FAILED to close", tr);
            }
        }

        protected void ResetSwordFish()
        {
            if (isSwordFishReset)
                return;

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


            string profit = "";
            if (DayProfitTotal != 0 && DayPipsTotal != 0)
            {
                profit = ("DAY TOTAL," + DayProfitTotal + "," + DayPipsTotal + "," + Time.DayOfWeek + "," + Time);
                debugCSV.Add(profit);
            }

            DayProfitTotal = 0;
            DayPipsTotal = 0;
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



        protected bool IsSwordFishTime()
        {
            return swordFishTimeInfo.IsPlacePendingOrdersTime(IsBacktesting, Server.Time);
        }

        protected void SetAllStopLosses(double SLPrice)
        {
            switch (LastPositionTradeType)
            {
                case TradeType.Buy:
                    SetStopLossForAllPositions(SLPrice - HardStopLossBuffer);
                    break;
                case TradeType.Sell:
                    SetStopLossForAllPositions(SLPrice + HardStopLossBuffer);
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
            while (PendingOrders.Count > 0)
            {
                TradeResult tr = CancelPendingOrder(PendingOrders[0]);
                if (!tr.IsSuccessful)
                    debug("FAILED to cancel", tr);

            }
            isPendingOrdersClosed = true;
        }

        protected void SetStopLossForAllPositions(double _stopLossPrice)
        {
            foreach (Position _p in Positions)
            {
                TradeResult tr = ModifyPosition(_p, _stopLossPrice, _p.TakeProfit);
                if (!tr.IsSuccessful)
                    debug("FAILED to modify SL", tr);
            }
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
            System.IO.File.WriteAllLines("C:\\Users\\alist\\Desktop\\swordfish-debug.csv", debugCSV.ToArray());
        }

        protected void debug(string msg, TradeResult tr)
        {
            if (tr.Position != null)
                Print(msg, " Position: ", tr.Position.Label, " ", tr.Position.TradeType, " ", Time);
            if (tr.PendingOrder != null)
                Print(msg, " Pending Order: ", tr.PendingOrder.Label, " ", tr.PendingOrder.TradeType, " ", Time);
        }


        protected string getTimeStamp()
        {
            return Time.Year + "-" + Time.Month + "-" + Time.Day;
        }


        protected void setTimeZone()
        {

            switch (Symbol.Code)
            {
                case "UK100":
                    // Instantiate a MarketTimeInfo object.
                    swordFishTimeInfo.market = "FTSE 100";
                    swordFishTimeInfo.tz = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
                    // Market for swordfish trades opens at 8:00am.
                    swordFishTimeInfo.open = new TimeSpan(8, 0, 0);
                    // Market for swordfish trades closes at 8:05am.
                    swordFishTimeInfo.close = new TimeSpan(8, 5, 0);
                    // Close all open Swordfish position at 11:29am before US opens.
                    swordFishTimeInfo.closeAll = new TimeSpan(11, 29, 0);

                    break;
                case "AUS200":
                    swordFishTimeInfo.tz = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
                    // Market for swordfish opens at 9:00.
                    swordFishTimeInfo.open = new TimeSpan(9, 0, 0);
                    // Market for swordfish closes at 9:05.
                    swordFishTimeInfo.close = new TimeSpan(9, 3, 0);
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









