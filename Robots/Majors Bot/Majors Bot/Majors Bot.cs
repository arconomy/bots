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
    public class MajorsBot : Robot
    {
        [Parameter("Modify Period", DefaultValue = 2)]
        public int _modifyPeriod { get; set; }

        [Parameter("Stop Loss", DefaultValue = 30)]
        public int _stopLoss { get; set; }

        [Parameter("Take Profit", DefaultValue = 3.0)]
        public int _takeProfit { get; set; }

        [Parameter("Entry Offset", DefaultValue = 30)]
        public int _entryOffset { get; set; }

        //swordfish params

        [Parameter("Source")]
        public DataSeries _dataSeriesSource { get; set; }

        [Parameter("Check Bollinger Bollinger Band", DefaultValue = false)]
        public bool _checkBollingerBand { get; set; }

        [Parameter("Pips inside Bollinger Band", DefaultValue = 2)]
        public int _targetEntryPips { get; set; }

        [Parameter("Initial Order placement trigger from open", DefaultValue = 5)]
        public int _swordFishTrigger { get; set; }

        [Parameter("Offset from Market Open for First Order", DefaultValue = 9)]
        public int _orderEntryOffset { get; set; }

        [Parameter("Distance between Orders in Pips", DefaultValue = 1)]
        public int _orderSpacing { get; set; }

        [Parameter("# of Limit Orders", DefaultValue = 40)]
        public int _numberOfOrders { get; set; }

        [Parameter("Volume (Lots)", DefaultValue = 1)]
        public int _volume { get; set; }

        [Parameter("Volume Max (Lots)", DefaultValue = 200)]
        public int _volumeMax { get; set; }

        [Parameter("# Order placed before Volume multiples", DefaultValue = 5)]
        public int _orderVolumeLevels { get; set; }

        [Parameter("Volume multipler", DefaultValue = 2)]
        public int _volumeMultipler { get; set; }

        [Parameter("Mins after swordfish period to reduce position risk", DefaultValue = 45)]
        public int _reducePositionRiskTime { get; set; }

        [Parameter("Enable Retrace risk management", DefaultValue = true)]
        public bool _retraceEnabled { get; set; }

        [Parameter("Retrace level 1 Percentage", DefaultValue = 33)]
        public int _retraceLevel1 { get; set; }

        [Parameter("Retrace level 2 Percentage", DefaultValue = 50)]
        public int _retraceLevel2 { get; set; }

        [Parameter("Retrace level 3 Percentage", DefaultValue = 66)]
        public int _retraceLevel3 { get; set; }

        [Parameter("Initial Hard SL for last Order placed", DefaultValue = 5)]
        public double _finalOrderStopLoss { get; set; }

        [Parameter("Triggered Hard SL buffer", DefaultValue = 20)]
        public double _hardStopLossBuffer { get; set; }

        [Parameter("Trailing SL fixed distance", DefaultValue = 5)]
        public double _trailingStopPips { get; set; }

        protected BollingerBands _boli;

        //Price and Position Variables
        protected double _openPrice;
        protected string _lastPositionLabel;
        protected TradeType _lastPositionTradeType;
        protected double _lastPositionEntryPrice;
        protected double _lastClosedPositionEntryPrice;
        protected double _lastProfitPrice;

        protected double _openedPositionsCount = 0;
        protected double _closedPositionsCount = 0;

        //Stop Loss Variables
        protected double _divideTrailingStopPips = 1;
        protected bool _isTrailingStopsActive = false;
        protected bool _isHardSLLastProfitPrice = false;
        protected bool _isHardSLLastPositionEntryPrice = false;
        protected bool _isHardSLLastClosedPositionEntryPrice = false;
        protected bool _isBreakEvenStopLossActive = false;

        //Swordfish State Variables
        protected bool _isPendingOrdersClosed = false;
        protected bool _openPriceCaptured = false;
        protected bool _ordersPlaced = false;
        protected bool _isBreakOutReset = true;
        protected bool _isBreakOutActive = false;
        protected bool _isReducedRiskTime = false;

        List<string> _debugCSV = new List<string>();

        //Performance Reporting
        protected double _dayProfitTotal = 0;
        protected double _dayPipsTotal = 0;

        protected override void OnStart()
        {
            Positions.Opened += PositionsOnOpened;
            Positions.Closed += PositionsOnClosed;
            Timer.Start(_modifyPeriod);
        }

        protected override void OnTick()
        {
            // If there is a breakout then manage position
            if (_isBreakOutActive)
            {
                ManagePositionRisk();
            }
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }

        protected void ManagePositionRisk()
        {

            // Need to work out how to determine whether the BreakOut is over and much time before allowing more Orders to be set.
        }

        protected override void OnTimer()
        {
            //check status
            if (!_isBreakOutActive)
            {
                if(_isBreakOutReset)
                {
                    // Create new Pending Stop Orders
                    for (int orderCount = 0; orderCount < _numberOfOrders; orderCount++)
                    {
                        CreatePendingOrders(orderCount);
                        _isBreakOutReset = false;
                        _ordersPlaced = true;
                    }
                }
                else
                {
                    ModifyPendingOrders();
                }
            }
        }

        protected void CreatePendingOrders(int orderCount)
        {
            TradeResult sellOrder = PlaceStopOrder(
                TradeType.Sell,
                Symbol,
                setVolume(orderCount),
                (Symbol.Bid - _entryOffset - orderCount * _orderSpacing),
                "BKT-SELL#" + orderCount + "-" + getTimeStamp(),
                setPendingOrderStopLossPips(orderCount, _numberOfOrders),
                _takeProfit * (1 / Symbol.TickSize));
            if (!sellOrder.IsSuccessful)
                debug("FAILED to place order", sellOrder);

            TradeResult buyOrder = PlaceStopOrder(
                TradeType.Buy, Symbol,
                setVolume(orderCount),
                (Symbol.Ask + _entryOffset + orderCount * _orderSpacing),
                "BKT-BUY#" + orderCount + "-" + getTimeStamp(),
                setPendingOrderStopLossPips(orderCount, _numberOfOrders),
                _takeProfit * (1 / Symbol.TickSize));
            if (!buyOrder.IsSuccessful)
                debug("FAILED to place order", buyOrder);
        }

        protected void ModifyPendingOrders()
        {
            int buyOrderCount = 0;
            int sellOrderCount = 0;

            //Sort the Pending Orders by label so that when they are modified the labels still align with the order in which the PendingOrders are placed.
            IEnumerable<PendingOrder> sortedPendingOrders = PendingOrders.OrderBy(PendingOrder => PendingOrder.Label);
            foreach (PendingOrder _po in sortedPendingOrders)
            {
                if (_po.TradeType == TradeType.Buy)
                {  
                    ModifyPendingOrder(_po, (Symbol.Ask + _entryOffset + buyOrderCount * _orderSpacing), _po.StopLossPips, _po.TakeProfitPips, null);
                    buyOrderCount++;
                }

                if (_po.TradeType == TradeType.Sell)
                {
                    ModifyPendingOrder(_po, (Symbol.Bid - _entryOffset - sellOrderCount * _orderSpacing), _po.StopLossPips, _po.TakeProfitPips, null);
                    sellOrderCount++;
                }
            }
        }

        protected void PositionsOnOpened(PositionOpenedEventArgs args)
        {
            _openedPositionsCount++;

            //Capture last Position Opened i.e. the furthest away
            _lastPositionTradeType = args.Position.TradeType;
            _lastPositionEntryPrice = args.Position.EntryPrice;
            _lastPositionLabel = args.Position.Label;

            // Once a position is open manage the Breakout
            _isBreakOutActive = true;
        }

        protected void PositionsOnClosed(PositionClosedEventArgs args)
        {
            _closedPositionsCount++;

            _dayProfitTotal += args.Position.GrossProfit;
            _dayPipsTotal += args.Position.Pips;
            _debugCSV.Add("TRADE," + args.Position.GrossProfit + "," + args.Position.Pips + "," + Time.DayOfWeek + "," + args.Position.Label + "," + args.Position.EntryPrice + "," + History.FindLast(args.Position.Label, Symbol, args.Position.TradeType).ClosingPrice + "," + args.Position.StopLoss + "," + args.Position.TakeProfit + "," + Time + debugState());

            //Last position's SL has been triggered for a loss - NOT a swordfish
            if (_lastPositionLabel == args.Position.Label && args.Position.GrossProfit < 0)
            {
                Print("CLOSING ALL POSITIONS due to first position losing");
                CloseAllPendingOrders();
                CloseAllPositions();
                _isBreakOutActive = false;
            }

            //Taking profit
            if (args.Position.GrossProfit > 0)
            {
                //capture last position take profit price
                setLastProfitPrice(args.Position.TradeType);

                //capture last closed position entry price
                _lastClosedPositionEntryPrice = args.Position.EntryPrice;

                //If the spike has retraced then close all pending and set trailing stop
                ManagePositionRisk();

                //BreakEven SL triggered in ManageRisk() function
                if (_isBreakEvenStopLossActive)
                {
                    setBreakEvens(_lastProfitPrice);
                }
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
            _isPendingOrdersClosed = true;
        }

        protected void setLastProfitPrice(TradeType lastProfitTradeType)
        {
            if (lastProfitTradeType == TradeType.Buy)
                _lastProfitPrice = Symbol.Ask;
            if (lastProfitTradeType == TradeType.Sell)
                _lastProfitPrice = Symbol.Bid;
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

        protected void setBreakEvens(double breakEvenTriggerPrice)
        {
            TradeResult tr;
            foreach (Position _p in Positions)
            {
                if (_lastPositionTradeType == TradeType.Buy)
                {
                    if (breakEvenTriggerPrice > _p.EntryPrice)
                    {
                        tr = ModifyPosition(_p, _p.EntryPrice + _hardStopLossBuffer, _p.TakeProfit);
                        if (!tr.IsSuccessful)
                            debug("FAILED to modify", tr);
                    }

                }
                if (_lastPositionTradeType == TradeType.Sell)
                {
                    if (breakEvenTriggerPrice < _p.EntryPrice)
                    {
                        tr = ModifyPosition(_p, _p.EntryPrice - _hardStopLossBuffer, _p.TakeProfit);
                        if (!tr.IsSuccessful)
                            debug("FAILED to modify", tr);
                    }
                }
            }
        }

        protected string getTimeStamp()
        {
            return Time.Year + "-" + Time.Month + "-" + Time.Day;
        }

        protected string debugState()
        { return ""; }

        protected int setVolume(int orderCount)
        {

            double orderVolumeLevel = orderCount / _orderVolumeLevels;
            double volume = Math.Pow(_volumeMultipler, orderVolumeLevel) * _volume;

            if (volume > _volumeMax)
            {
                volume = _volumeMax;
            }

            return (int)volume;
        }

       

        protected void debug(string msg, TradeResult tr)
        {
            if (tr.Position != null)
                Print(msg, " Position: ", tr.Position.Label, " ", tr.Position.TradeType, " ", Time);
            if (tr.PendingOrder != null)
                Print(msg, " Pending Order: ", tr.PendingOrder.Label, " ", tr.PendingOrder.TradeType, " ", Time);
        }

        protected void ResetBreakOut()
        {
            if (_isBreakOutReset)
                return;

            //reset position counters
            _openedPositionsCount = 0;
            _closedPositionsCount = 0;

            //reset Last Position variables
            _lastPositionLabel = "NO LAST POSITION SET";
            _lastPositionEntryPrice = 0;
            _lastClosedPositionEntryPrice = 0;
            _lastProfitPrice = 0;

            //reset risk management variables
            _divideTrailingStopPips = 1;
            _isTrailingStopsActive = false;
            _isBreakEvenStopLossActive = false;
            _isHardSLLastClosedPositionEntryPrice = false;
            _isHardSLLastPositionEntryPrice = false;
            _isHardSLLastProfitPrice = false;

            // swordfish bot state variables
            _openPriceCaptured = false;
            _ordersPlaced = false;
            _isPendingOrdersClosed = false;
            _isBreakOutActive = false;
            _isBreakOutReset = true;
            _isReducedRiskTime = false;

            string profit = "";
            if (_dayProfitTotal != 0 && _dayPipsTotal != 0)
            {
                profit = ("DAY TOTAL," + _dayProfitTotal + "," + _dayPipsTotal + "," + Time.DayOfWeek + "," + Time);
                _debugCSV.Add(profit);
            }

            _dayProfitTotal = 0;
            _dayPipsTotal = 0;
        }

        //Set a stop loss on the last Pending Order set to catch the break away train that never comes back!
        protected double setPendingOrderStopLossPips(int orderCount, int numberOfOrders)
        {
            if (orderCount == _numberOfOrders - 1)
            {
                return _finalOrderStopLoss * (1 / Symbol.TickSize);
            }
            else
            {
                return 0;
            }
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


    }
}

/*
    bool HasBuyPosition = false;
    bool HasSellPosition = false;
    foreach (Position position in Positions)
    {

        if (position.SymbolCode != Symbol.Code)
            continue;

        if (position.Label == "BUY")
        {
            HasBuyPosition = true;
        }
        else
        {
            HasSellPosition = true;
        }
    }


    //If we dont have a Buy Position...
    if (!HasBuyPosition)
    {

        // IF we have a Pending BUY then MOdify...
        bool HasBuyOrder = false;
        foreach (PendingOrder PO in PendingOrders)
        {
            if (PO.SymbolCode != Symbol.Code)
                continue;

            if (PO.TradeType == TradeType.Buy)
            {
                HasBuyOrder = true;
                ModifyPendingOrder(PO, (Symbol.Ask + EntryOffset), StopLoss, TakeProfit, null);
            }
        }
        // If we dont have a Pending Buy then create a new one!
        if (!HasBuyOrder)
        {
            var Result = PlaceStopOrder(TradeType.Buy, Symbol, Volume, (Symbol.Ask + EntryOffset), "BUY", StopLoss, TakeProfit);

        }

    }

    //Make some monay!!

    if (!HasSellPosition)
    {
        bool HasSellOrder = false;
        foreach (PendingOrder PO in PendingOrders)
        {

            if (PO.SymbolCode != Symbol.Code)
                continue;

            if (PO.TradeType == TradeType.Sell)
            {
                HasSellOrder = true;
                ModifyPendingOrder(PO, (Symbol.Bid - EntryOffset), StopLoss, TakeProfit, null);
            }
        }

        if (!HasSellOrder)
            PlaceStopOrder(TradeType.Sell, Symbol, Volume, (Symbol.Bid - EntryOffset), "SELL", StopLoss, TakeProfit);
*/

//Increase the volume based on Orders places and volume levels and multiplier until max volume reached
/*

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


        

       



        protected override void OnStop()
        {
            // Put your deinitialization logic here
            System.IO.File.WriteAllLines("C:\\Users\\alist\\Desktop\\swordfish-debug.csv", debugCSV.ToArray());
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


*/






