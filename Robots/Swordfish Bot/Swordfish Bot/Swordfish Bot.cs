﻿using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
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

        [Parameter("# of Limit Orders", DefaultValue = 20)]
        public int NumberOfOrders { get; set; }

        [Parameter("Volume (Lots)", DefaultValue = 1, MinValue = 0.01, Step = 0.01)]
        public int Volume { get; set; }

        [Parameter("Take Profit", DefaultValue = 0.8)]
        public double TakeProfit { get; set; }

        [Parameter("% retracement to trigger setting trailing SL", DefaultValue = 33)]
        public double RetracePercentage { get; set; }

        [Parameter("% of Positions closed by retrace to trigger setting trailing SL", DefaultValue = 40)]
        public double PercentageOfPositionsClosed { get; set; }

        [Parameter("Mins after swordfish period to reduce position risk", DefaultValue = 45)]
        public int ReducePositionRiskTime { get; set; }

        [Parameter("Initial Hard SL for last Order placed", DefaultValue = 2)]
        public double FinalOrderStopLoss { get; set; }

        [Parameter("Triggered Hard SL buffer", DefaultValue = 2)]
        public double HardStopLossBuffer { get; set; }

        [Parameter("Trailing SL fixed distance", DefaultValue = 10)]
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


        protected int OpenedPositionsCount = 0;
        protected int ClosedPositionsCount = 0;   

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
        protected bool isSwordFishReset = false;

        
        protected override void OnStart()
        {
            swordFishTimeInfo = new MarketTimeInfo();
            setTimeZone();
            Boli = Indicators.BollingerBands(DataSeriesSource, 2, 20, MovingAverageType.Exponential);

            Positions.Opened += PositionsOnOpened;
            Positions.Closed += PositionsOnClosed;

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
                    Print("OPEN PRICE CAPTURED: ", OpenPrice);
                }

                if(!OrdersPlaced)
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
                                    PlaceLimitOrder(TradeType.Sell, Symbol, Volume, (OpenPrice + OrderEntryOffset + OrderCount * OrderSpacing), "SWORDFISH#" + OrderCount + "-" + getTimeStamp(), setPendingOrderStopLoss(OrderCount, NumberOfOrders), TakeProfit * (1 / Symbol.TickSize));
                                }
                            }
                            else
                            {
                                TradeResult SellLimitOrder = PlaceLimitOrder(TradeType.Sell, Symbol, Volume, (OpenPrice + OrderEntryOffset + OrderCount * OrderSpacing), "SWORDFISH#" + OrderCount + "-" + getTimeStamp(), setPendingOrderStopLoss(OrderCount, NumberOfOrders), TakeProfit * (1 / Symbol.TickSize));
                                if (!SellLimitOrder.IsSuccessful)
                                    debug("FAILED to place order", SellLimitOrder);
                            }
                        }
                        //All Sell Stop Orders have been placed
                        OrdersPlaced = true;
                    }
                    //Price moves 5pts DOWN from open then look to set BUY LimitOrders
                    else if (OpenPrice - SwordFishTrigger > Symbol.Ask)
                    {
                        //Place Buy Limit Orders
                        for (int OrderCount = 0; OrderCount < NumberOfOrders - 1; OrderCount++)
                        {
                            //confirm last bar broke the Bollinger Band Top indicating overbought - OPTIONAL
                            if (checkBollingerBand)
                            {
                                //OPTIONAL - Confirm last bar broke the Bollinger Band Bottom indicating market is oversold
                                if (Boli.Bottom.Last(0) < MarketSeries.Close.LastValue)
                                {
                                    // Place BUY Limit order spaced by OrderSpacing 
                                    PlaceLimitOrder(TradeType.Buy, Symbol, Volume, (OpenPrice + OrderEntryOffset + OrderCount * OrderSpacing), "SWORDFISH#" + OrderCount + "-" + getTimeStamp(), setPendingOrderStopLoss(OrderCount, NumberOfOrders), TakeProfit * (1 / Symbol.TickSize));
                                }

                            }
                            else
                            {
                                TradeResult BuyLimitOrder = PlaceLimitOrder(TradeType.Buy, Symbol, Volume, (OpenPrice - OrderEntryOffset - OrderCount * OrderSpacing), "SWORDFISH#" + OrderCount + "-" + getTimeStamp(), setPendingOrderStopLoss(OrderCount, NumberOfOrders), TakeProfit * (1 / Symbol.TickSize));
                                if (!BuyLimitOrder.IsSuccessful)
                                    debug("FAILED to place order", BuyLimitOrder);
                            }
                        }
                        //All Buy Stop Orders have been placed
                        OrdersPlaced = true;
                    }
                }
            }
            else //It is outside SwordFish Time
            {
                if (OrdersPlaced)
                {
                    if (Positions.Count > 0)
                    {
                        //Look to reduce risk as Spike retraces
                        ManagePositionRisk();

                        //Positions still open after ReducePositionRiskTime
                        if (swordFishTimeInfo.IsReduceRiskTime(IsBacktesting, Server.Time, ReducePositionRiskTime))
                        {
                            Print("---- REDUCE POSITION RISK 50% out of Swordfish CLOSING TIME: ", Time);
                            //Reduce Trailing Stop Loss by 50%
                            DivideTrailingStopPips = 2;
                        }

                        //If trades still open at ClosingAllTime then take the hit and close remaining positions
                        if (swordFishTimeInfo.IsCloseAllPositionsTime(IsBacktesting, Server.Time) && !isSwordfishTerminated)
                        {
                            Print("---- CLOSE ALL POSITIONS out of Swordfishtime: ", Time);
                            CloseAllPositions();
                            isSwordfishTerminated = true;
                        }
                    }
                    else
                    {
                        //No positions opened and out of Swordfish time
                        CloseAllPendingOrders();
                        ResetSwordFish();
                    }

                    //Out of Swordfish time and all positions that opened are now closed
                    if (OpenedPositionsCount > 0 && OpenedPositionsCount == ClosedPositionsCount)
                        ResetSwordFish();
                }
                else //No Orders placed therefore reset Swordfish
                {
                    ResetSwordFish();
                }
            }
            
            if (isTrailingStopsActive)
            {
                    double newStopLoss = 0;
                    double currentStopLoss;
                    foreach (Position _p in Positions)
                    {

                        bool isProtected = _p.StopLoss.HasValue;
                        if (isProtected)
                        {
                            currentStopLoss = (double)_p.StopLoss;
                        }
                        else
                        {
                            //Should never happen
                            Print("WARNING: Trail Activated but No intial STOP LESS set");
                            currentStopLoss = LastPositionEntryPrice;
                        }

                        if (_p.TradeType == TradeType.Buy)
                        {
                            newStopLoss = Symbol.Ask - TrailingStopPips / DivideTrailingStopPips;

                            //Is newStopLoss more risk than current SL
                            if (newStopLoss < currentStopLoss)
                                return;

                            //Is newStopLoss more than the current Ask and therefore not valid
                            if (newStopLoss > Symbol.Ask)
                                return;

                            //Is the difference between the newStopLoss and the current SL less than the tick size and therefore not valid
                            if (newStopLoss - _p.StopLoss < Symbol.TickSize)
                                return;
                        }

                        if (_p.TradeType == TradeType.Sell)
                        {
                            newStopLoss = Symbol.Bid + TrailingStopPips / DivideTrailingStopPips;

                            //Is newStopLoss more risk than current SL
                            if (newStopLoss > currentStopLoss)
                                return;

                            //Is newStopLoss more than the current Ask and therefore not valid
                            if (newStopLoss < Symbol.Bid)
                                return;

                            //Is the difference between the newStopLoss and the current SL less than the tick size and therefore not valid
                            if (_p.StopLoss - newStopLoss < Symbol.TickSize)
                                return;
                        }

                        TradeResult tr = ModifyPosition(_p, newStopLoss, _p.TakeProfit);
                        if (!tr.IsSuccessful)
                            debug("FAILED to modify SL", tr);

                }
            }
        }

        protected void ManagePositionRisk()
        {
            //Close any positions that have not been triggered
            if(!isPendingOrdersClosed)
             CloseAllPendingOrders();

            //Calculate spike retrace factor
            double retraceFactor = (calculatePercentageClosed() + calculatePercentageRetrace())/2;

            //If it has retraced less than 25%
            if (25 > retraceFactor)
            {
                //Set hard stop losses
                if (!isHardSLLastPositionEntryPrice)
                {
                    SetAllStopLosses(LastPositionEntryPrice);
                    isHardSLLastPositionEntryPrice = true;
                }
            }

            //If it has retraced between than 25% and 50%
            if(50 > retraceFactor && retraceFactor > 25)
            {                
                //Activate Trailing Stop Losses
                isTrailingStopsActive = true;
            }

            //If it has retraced between than 50% and 75%
            else if(75 > retraceFactor && retraceFactor > 50)
            {
                //Set hard stop losses
                if (!isHardSLLastClosedPositionEntryPrice)
                {
                    SetAllStopLosses(LastClosedPositionEntryPrice);
                    isHardSLLastClosedPositionEntryPrice = true;
                }

                //Active Breakeven Stop Losses
                isBreakEvenStopLossActive = true;
            }
            else if (100 > retraceFactor && retraceFactor > 75)
            {
                //Set hard stop losses
                if (!isHardSLLastProfitPrice)
                {
                    SetAllStopLosses(LastProfitPrice);
                    isHardSLLastProfitPrice = true;
                }
            }
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

            Print("*** CLOSED POSITION: ",args.Position.Label," Profit: ", args.Position.GrossProfit," Entry: ", args.Position.EntryPrice, " SL: ", args.Position.StopLoss, " TP: ", args.Position.TakeProfit);
            foreach(Position p in Positions)
            {
                Print(p.Label, " Entry: ", p.EntryPrice, " SL: ", p.StopLoss, " TP: ", p.TakeProfit);
            }
            debugState();
            Print("********************************************");

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
                        Print("---- Modifying SL to BREAKEVEN ----", _p.Label);
                        tr = ModifyPosition(_p, _p.EntryPrice + HardStopLossBuffer, _p.TakeProfit);
                        if (!tr.IsSuccessful)
                            debug("FAILED to modify", tr);
                    }

                }
                if (LastPositionTradeType == TradeType.Sell)
                {
                    if (breakEvenTriggerPrice < _p.EntryPrice)
                    {
                        Print("---- Modifying SL to BREAKEVEN ----", _p.Label);
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

            Print("******* RESET SWORDFISH: ", Time);
        }

        protected void debugState()
        {
            // Position counters
            Print("OpenedPositionsCount = ",OpenedPositionsCount);
            Print("ClosedPositionsCount = ", ClosedPositionsCount);

            // Last Position variables
            Print("LastPositionEntryPrice = ", LastPositionEntryPrice);
            Print("LastProfitPrice = ", LastProfitPrice);
            Print("LastPositionLabel = ", LastPositionLabel);

            // risk management variables
            Print("isSwordfishTerminated = ", isSwordfishTerminated);
            Print("DivideTrailingStopPips = ", DivideTrailingStopPips);
            Print("isTrailingStopsActive = ", isTrailingStopsActive);
            Print("isBreakEvenStopLossActive = ", isBreakEvenStopLossActive);

            // swordfish bot state variables
            Print("OpenPriceCaptured = ", OpenPriceCaptured);
            Print("OrdersPlaced = ", OrdersPlaced);
            Print("isSwordFishReset = ", isSwordFishReset);
        }



        protected bool IsSwordFishTime()
        {
            return swordFishTimeInfo.IsPlacePendingOrdersTime(IsBacktesting, Server.Time);
        }

        protected void SetAllStopLosses(double SL)
        {
            switch (LastPositionTradeType)
            {
                case TradeType.Buy:
                    SetStopLossForAllPositions(SL - HardStopLossBuffer);
                    break;
                case TradeType.Sell:
                    SetStopLossForAllPositions(SL + HardStopLossBuffer);
                    break;
            }
        }

        protected bool HasSpikeRetraced(double priceRetracePercentage, double positionClosedPercentage)
        {
            //Test if either the PercentageOfPositionsClosed or RetracePercentage threshold has passed
            return (calculatePercentageClosed() > positionClosedPercentage || calculatePercentageRetrace() > priceRetracePercentage);
        }

        protected double calculatePercentageRetrace()
        {
            if (LastPositionEntryPrice > OpenPrice)
            {
                //Position are Selling
                return Symbol.Bid - OpenPrice / (LastPositionEntryPrice - OpenPrice);
            }
            else
            {
                //Positions are buying
                return OpenPrice - Symbol.Bid / (OpenPrice - LastPositionEntryPrice);
            }
        }

        protected double calculatePercentageClosed()
        {
            if (OpenedPositionsCount > 0)
            {
                return (ClosedPositionsCount / OpenedPositionsCount) * 100;
            }
            else
            {
                return 0;
            }
        }


        //Set a stop loss on the last Pending Order set to catch the break away train that never comes back!
        protected double setPendingOrderStopLoss(int _orderCount, int _numberOfOrders)
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

        protected void SetStopLossForAllPositions(double _stopLoss)
        {
            foreach (Position _p in Positions)
            {
                TradeResult tr = ModifyPosition(_p, _stopLoss, _p.TakeProfit);
                if (!tr.IsSuccessful)
                    debug("FAILED to modify SL", tr);
            }
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
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
    public bool IsReduceRiskTime(bool _isBackTesting, DateTime _serverTime, int timeFromOpen)
    {
        if (_isBackTesting)
        {
            return IsOpenAt(_serverTime.Add(TimeSpan.FromMinutes(-timeFromOpen)));
        }
        else
        {
            return IsOpenAt(DateTime.UtcNow.Add(TimeSpan.FromMinutes(-timeFromOpen)));
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

    //Is the current time within the period Swordfish Pending Orders can be placed or during the time period when risk should be reduced.
    public bool IsOpenAt(DateTime DateTimeNow)
    {
        DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(DateTimeNow, tz);
        return tzTime.TimeOfDay >= open & tzTime.TimeOfDay <= close;
    }

    //Is the current time within the period Swordfish positions can remain open.
    public bool IsCloseAllAt(DateTime DateTimeNow)
    {
        DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(DateTimeNow, tz);
        return tzTime.TimeOfDay >= closeAll;
    }


}








