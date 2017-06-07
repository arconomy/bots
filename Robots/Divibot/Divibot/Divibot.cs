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
    public class Divibot : Robot
    {

        [Parameter("Source")]
        public DataSeries DataSeriesSource { get; set; }

        [Parameter("# of Limit Orders", DefaultValue = 15)]
        public int NumberOfSellLimitOrders { get; set; }

        [Parameter("# of Sell Trades placed", DefaultValue = 10)]
        public int NumberOfPositions { get; set; }

        [Parameter("Volume (Lots)", DefaultValue = 20)]
        public int Volume { get; set; }

        [Parameter("Volume Max (Lots)", DefaultValue = 100)]
        public int VolumeMax { get; set; }

        [Parameter("# Order placed before Volume multiplies", DefaultValue = 2)]
        public int OrderVolumeLevels { get; set; }

        [Parameter("Volume multipler", DefaultValue = 2)]
        public double VolumeMultipler { get; set; }

        [Parameter("Take profity interval spacing in Pips", DefaultValue = 0.5)]
        public double TPSpacing { get; set; }

        [Parameter("# positions open before TP spacing increases by multiplier", DefaultValue = 20)]
        public int TPSpacingLevels { get; set; }

        [Parameter("Take profit spacing multipler", DefaultValue = 2)]
        public double TPSpacingMultipler { get; set; }

        [Parameter("Take profit spacing max", DefaultValue = 3)]
        public int TPSpacingMax { get; set; }

        [Parameter("Maximum Take Profit", DefaultValue = 0.5)]
        public double MaxTakeProfit { get; set; }

        [Parameter("Minimum Take Profit", DefaultValue = 0.3)]
        public double MinTakeProfit { get; set; }

        [Parameter("Mins after trading start reduce position risk", DefaultValue = 10)]
        public int ReducePositionRiskTime { get; set; }

        [Parameter("Enable chase risk management", DefaultValue = true)]
        public bool chaseEnabled { get; set; }

        [Parameter("Chase level Percentage", DefaultValue = 20)]
        public int chaseLevel1 { get; set; }

        [Parameter("Chase level Percentage", DefaultValue = 70)]
        public int chaseLevel2 { get; set; }

        [Parameter("Initial Hard SL for last Order placed", DefaultValue = 5)]
        public double FinalOrderStopLoss { get; set; }

        [Parameter("Triggered Chase Level 2 Hard SL", DefaultValue = 20)]
        public double HardStopLoss { get; set; }

        [Parameter("Trailing SL fixed distance", DefaultValue = 5)]
        public double TrailingStopPips { get; set; }

        protected MarketTimeInfo _marketTimeInfo;

        //Price and Position Variables
        protected double _startPrice;
        protected double _earlyEntryPrice;
        protected string _lastPositionLabel;
        protected TradeType _lastPositionTradeType;
        protected double _lastPositionEntryPrice;
        protected double _lastClosedPositionEntryPrice;
        protected double _lastProfitPrice;

        protected double _openedPositionsCount = 0;
        protected double _closedPositionsCount = 0;
        protected double _orderCountLabel = 0;

        //Stop Loss Variables
        protected double _divideTrailingStopPips = 1;
        protected bool _isTrailingStopsActive = false;
        protected bool _isHardSLLastProfitPrice = false;
        protected bool _isHardSLLastPositionEntryPrice = false;
        protected bool _isHardSLActive = false;
        protected bool _isBreakEvenStopLossActive = false;

        //Swordfish State Variables
        protected bool _isPendingOrdersClosed = false;
        protected bool _startPriceCaptured = false;
        protected bool _earlyEntryPriceCaptured = false;
        protected bool _ordersPlaced = false;
        protected bool _positionsPlaced = false;
        protected bool _isTerminated = false;
        protected bool _isReset = true;
        protected bool _isReducedRiskTime = false;

        protected string _botId = null;

        List<string> debugCSV = new List<string>();

        //Performance Reporting
        protected double _dayProfitTotal = 0;
        protected double _dayPipsTotal = 0;
        protected double _spikePeakPips = 0;
        protected double _spikePeakPrice = 0;

        protected override void OnStart()
        {
            _botId = generateBotId();
            _marketTimeInfo = new MarketTimeInfo();
            setTimeZone();

            Positions.Opened += PositionsOnOpened;
            Positions.Closed += PositionsOnClosed;
            debugCSV.Add("PARAMETERS");
            debugCSV.Add("NumberOfOrders," + NumberOfPositions.ToString());
            debugCSV.Add("Volume," + Volume.ToString());
            debugCSV.Add("VolumeMax," + VolumeMax.ToString());
            debugCSV.Add("OrderVolumeLevels," + OrderVolumeLevels.ToString());
            debugCSV.Add("VolumeMultipler," + VolumeMultipler.ToString());
            debugCSV.Add("MaxTakeProfit," + MaxTakeProfit.ToString());
            debugCSV.Add("MinTakeProfit," + MinTakeProfit.ToString());
            debugCSV.Add("ReducePositionRiskTime," + ReducePositionRiskTime.ToString());
            debugCSV.Add("ChaseEnabled," + chaseEnabled.ToString());
            debugCSV.Add("ChaseLevel," + chaseLevel1.ToString());
            debugCSV.Add("FinalOrderStopLoss," + FinalOrderStopLoss.ToString());
            debugCSV.Add("HardStopLossBuffer," + HardStopLoss.ToString());
            debugCSV.Add("TrailingStopPips," + TrailingStopPips.ToString());
            debugCSV.Add("--------------------------");

            debugCSV.Add("Label," + "Profit," + "Pips," + "EntryPrice," + "ClosePrice," + "SL," + "TP," + "Day," + "Date/Time," + "OpenedPositionsCount," + "ClosedPositionsCount,," + "LastPositionEntryPrice,LastClosedPositionEntryPrice,LastProfitPrice," + "LastPositionLabel,DivideTrailingStopPips,isTrailingStopsActive,isBreakEvenStopLossActive," + "isHardSLLastClosedPositionEntryPrice,isHardSLLastPositionEntryPrice,isHardSLLastProfitPrice,StartPriceCaptured," + "OrdersPlaced,isReset,isTerminated,isReducedRiskTime");
        }

        protected string generateBotId()
        {
            Random randomIdGenerator = new Random();
            int id = randomIdGenerator.Next(0, 99999);
            return id.ToString("00000");
        }

        protected override void OnTick()
        {
            if (IsTradingTimeIn(4))
            {
                if (!_earlyEntryPriceCaptured)
                {
                    //Get the Price 5mins before open
                    _earlyEntryPrice = Symbol.Ask;
                    _earlyEntryPriceCaptured = true;
                }
            }

            // If backtesting use the Server.Time.        
            if (IsTradingTime())
            {
                //Start Trading
                if (_isReset)
                    _isReset = false;

                if (!_startPriceCaptured)
                {
                    //Get the Market Open Price
                    _startPrice = MarketSeries.Close.LastValue;
                    _startPriceCaptured = true;
                }

                if (!_positionsPlaced && _earlyEntryPrice - 3 < Symbol.Ask)
                {
                    placeSellOrders();
                }

                if (!_ordersPlaced)
                {
                    placeSellLimitOrders();
                }

                captureSpikePeak();
            }
            //It is outside Placing Trading Time
            else
            {
                //Cancel all open pending Orders
                CancelAllPendingOrders();

                if (_ordersPlaced || _positionsPlaced)
                {
                    if (_openedPositionsCount - _closedPositionsCount > 0)
                    {
                        //Positions still open after ReducePositionRiskTime
                        if (!_isReducedRiskTime && _marketTimeInfo.IsReduceRiskTime(IsBacktesting, Server.Time, ReducePositionRiskTime))
                        {
                            _isReducedRiskTime = true;
                        }

                        //If trades still open at ClosingAllTime then take the hit and close remaining positions
                        if (!_isTerminated && _marketTimeInfo.IsCloseAllPositionsTime(IsBacktesting, Server.Time))
                        {
                            CloseAllPositions();
                            _isTerminated = true;
                        }

                        //Manage the open positions
                        ManagePositionRisk();
                    }


                    //Out of Trading time and all positions that were opened are now closed
                    if (_openedPositionsCount > 0 && _openedPositionsCount - _closedPositionsCount == 0)
                        ResetSwordFish();
                }
            }
        }

        protected void ManagePositionRisk()
        {

            if (chaseEnabled)
            {
                //Calculate spike retrace factor
                double chaseFactor = calculateChaseFactor();

                //Activate BreakEven SL if chaseLevel1 has been passed
                if (chaseLevel1 < chaseFactor)
                {
                    //Activate Trailing Stop Losses
                    _isBreakEvenStopLossActive = true;
                }

                //Activate HardSL if chaseLevel2 has been passed
                if (chaseLevel2 < chaseFactor)
                {
                    //Activate Hard SL
                    _isHardSLActive = true;
                    if (_isHardSLActive)
                    {
                        setStopLossForAllPositions(HardStopLoss);
                    }
                }

                //Try and set a BreakEvenSL as soon as trades start taking profit
                if (_isBreakEvenStopLossActive)
                {
                    setBreakEvenSL();
                }

                // If Trailing stop is active update position SL's - Remove TP as trailing position.
                if (_isTrailingStopsActive)
                {
                    setTrailingSL();
                }
            }
        }

        public void CancelAllPendingOrders()
        {
            //Close any outstanding pending orders
            foreach (PendingOrder po in PendingOrders)
            {
                try
                {
                    if (isThisBotId(po.Label))
                    {
                        CancelPendingOrderAsync(po, OnPendingOrderCancelledComplete);
                    }
                } catch (Exception e)
                {
                    Print("Failed to Cancel Pending Order :" + e.Message);
                }
            }
            _isPendingOrdersClosed = true;
        }


        protected void captureSpikePeak()
        {
            //Capture the highest point of the Spike within trading Time
            if (_openedPositionsCount > 0)
            {
                //We are selling - look for the lowest point
                if (Symbol.Bid < _spikePeakPrice || _spikePeakPrice == 0)
                {
                    _spikePeakPrice = Symbol.Bid;
                    _spikePeakPips = _startPrice - Symbol.Bid;
                }
            }
        }

        protected void setTrailingSL()
        {
            foreach (Position p in Positions)
            {
                {
                    try
                    {
                        if (isThisBotId(p.Label))
                        {
                            double newStopLossPrice = calcTrailingStopLoss(p);
                            if (newStopLossPrice > 0)
                            {
                                ModifyPositionAsync(p, newStopLossPrice, p.TakeProfit, OnModifyTrailingStop);
                            }
                        }
                    } catch (Exception e)
                    {
                        Print("Failed to Modify Position:" + e.Message);
                    }
                }
            }
        }

        protected void setBreakEvenSL()
        {
            foreach (Position p in Positions)
            {
                //check if SL is already better than break even
                if (isCurrentSLCloser(p, p.EntryPrice))
                    continue;

                try
                {
                    if (isThisBotId(p.Label))
                    {
                        if (_lastPositionTradeType == TradeType.Buy)
                        {
                            if (Symbol.Ask - MinTakeProfit * (1/Symbol.TickSize) > p.EntryPrice)
                            {
                                ModifyPositionAsync(p, p.EntryPrice, p.TakeProfit, OnModifyBreakEvenStopComplete);
                            }
                        }

                        if (_lastPositionTradeType == TradeType.Sell)
                        {
                            if (Symbol.Bid + MinTakeProfit * (1 / Symbol.TickSize) < p.EntryPrice)
                            {
                                ModifyPositionAsync(p, p.EntryPrice, p.TakeProfit, OnModifyBreakEvenStopComplete);
                            }
                        }
                    }
                } catch (Exception e)
                {
                    Print("Failed to Modify Position:" + e.Message);
                }
            }
        }

        protected void setStopLossForAllPositions(double stopLossPrice)
        {
            foreach (Position p in Positions)
            {
                //check if SL is already better than break even
                if (isCurrentSLCloser(p, stopLossPrice))
                    continue;

                try
                {
                    if (isThisBotId(p.Label))
                    {
                        ModifyPositionAsync(p, stopLossPrice, p.TakeProfit, OnModifyHardSLComplete);
                    }
                } catch (Exception e)
                {
                    Print("Failed to Modify Position: " + e.Message);
                }
            }
        }



        protected bool isCurrentSLCloser(Position p, double newSLValue)
        {
            if (p.StopLoss.HasValue)
            {
                switch (p.TradeType)
                {
                    case TradeType.Sell:
                        {
                            return p.StopLoss.Value < newSLValue;
                        }
                    case TradeType.Buy:
                        {
                            return p.StopLoss.Value > newSLValue;
                        }
                    default:
                        return false;
                }
            }
            else
            {
                return false;
            }
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
                currentStopLossPrice = _lastPositionEntryPrice;
            }

            if (position.TradeType == TradeType.Buy)
            {
                newStopLossPrice = Symbol.Ask - TrailingStopPips / _divideTrailingStopPips;
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
                newStopLossPrice = Symbol.Bid + TrailingStopPips / _divideTrailingStopPips;
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

        // Place Sell Orders
        protected void placeSellOrders()
        {
            //Place Sell Limit Orders
            for (int OrderCount = 0; OrderCount < NumberOfPositions; OrderCount++)
            {
                try
                {
                    tradeData data = new tradeData 
                    {
                        tradeType = TradeType.Sell,
                        symbol = Symbol,
                        volume = setVolume(OrderCount),
                        entryPrice = 0,
                        label = _botId + "-" + getTimeStamp() + _marketTimeInfo.market + "-SWF#" + _orderCountLabel,
                        stopLossPips = FinalOrderStopLoss,
                        takeProfitPips = calcTakeProfit(OrderCount)
                    };
                    if (data == null)
                        continue;

                    //Place Market Orders immediately
                    ExecuteMarketOrderAsync(data.tradeType, data.symbol, data.volume, data.label + "XX", data.stopLossPips, data.takeProfitPips, OnPlaceTradeOperationComplete);
                    _orderCountLabel++;
                } catch (Exception e)
                {
                    Print("Failed to place Sell Limit Order: " + e.Message);
                }
            }
        }

        // Place Sell Limit Orders
        protected void placeSellLimitOrders()
        {
            //Place Sell Limit Orders
            for (int OrderCount = 0; OrderCount < NumberOfSellLimitOrders; OrderCount++)
            {
                try
                {
                    tradeData data = new tradeData 
                    {
                        tradeType = TradeType.Sell,
                        symbol = Symbol,
                        volume = setVolume(OrderCount),
                        entryPrice = calcSellEntryPrice(OrderCount),
                        label = _botId + "-" + getTimeStamp() + _marketTimeInfo.market + "-SWF#" + OrderCount,
                        stopLossPips = setPendingOrderStopLossPips(OrderCount, NumberOfSellLimitOrders),
                        takeProfitPips = calcTakeProfit(OrderCount)
                    };
                    if (data == null)
                        continue;

                    //Check that entry price is valid
                    if (data.entryPrice > Symbol.Ask)
                    {
                        PlaceLimitOrderAsync(data.tradeType, data.symbol, data.volume, data.entryPrice, data.label, data.stopLossPips, data.takeProfitPips, OnPlaceOrderOperationComplete);
                    }
                    else
                    {
                        //Tick price has 'jumped' - therefore avoid placing all PendingOrders by re-calculating the OrderCount to the equivelant entry point.
                        OrderCount = calculateNewOrderCount(NumberOfSellLimitOrders, OrderCount, Symbol.Ask);
                        ExecuteMarketOrderAsync(data.tradeType, data.symbol, data.volume, data.label + "X", data.stopLossPips, data.takeProfitPips, OnPlaceOrderOperationComplete);
                    }
                } catch (Exception e)
                {
                    Print("Failed to place Sell Limit Order: " + e.Message);
                }
            }
        }

        protected double calcSellEntryPrice(int orderCount)
        {
            return _startPrice + orderCount;
        }

        //Set a stop loss on the last Pending Order set to catch the break away train that never comes back!
        protected double setPendingOrderStopLossPips(int orderCount, int numberOfOrders)
        {
            if (orderCount == numberOfOrders - 1)
            {
                return FinalOrderStopLoss * (1 / Symbol.TickSize);
            }
            else
            {
                return 0;
            }
        }

        //Calculate a new orderCount number for when tick jumps
        protected int calculateNewOrderCount(int numberOfOrders, int orderCount, double currentTickPrice)
        {
            double tickJumpIntoRange = Math.Abs(_startPrice - currentTickPrice);
            double pendingOrderRange = numberOfOrders;
            //assume orders are placed at 1pip intervals
            double pendingOrdersPercentageJumped = tickJumpIntoRange / pendingOrderRange;
            double newOrderCount = numberOfOrders * pendingOrdersPercentageJumped;

            if (newOrderCount > orderCount)
                return (int)newOrderCount;
            else
                return (int)orderCount;
        }

        protected void OnPendingOrderCancelledComplete(TradeResult tr)
        {
            OnTradeOperationComplete(tr, "FAILED to CANCEL pending order : ");
        }

        protected void OnModifyHardSLComplete(TradeResult tr)
        {
            OnTradeOperationComplete(tr, "FAILED to modify HARD stop loss: ");
        }

        protected void OnModifyBreakEvenStopComplete(TradeResult tr)
        {
            OnTradeOperationComplete(tr, "FAILED to modify BREAKEVEN stop loss: ");
        }

        protected void OnModifyTakeProfitComplete(TradeResult tr)
        {
            OnTradeOperationComplete(tr, "FAILED to modify TAKE PROFIT: ");
        }

        protected void OnClosePositionComplete(TradeResult tr)
        {
            OnTradeOperationComplete(tr, "FAILED to close position: ");
        }

        protected void OnModifyTrailingStop(TradeResult tr)
        {
            OnTradeOperationComplete(tr, "FAILED to modify TRAILING stop loss: ");
        }

        protected void OnPlaceOrderOperationComplete(TradeResult tr)
        {
            if (tr.IsSuccessful)
                _ordersPlaced = true;
            else
                OnTradeOperationComplete(tr, "FAILED to place ORDER: ");
        }

        protected void OnPlaceTradeOperationComplete(TradeResult tr)
        {
            if (tr.IsSuccessful)
                _positionsPlaced = true;
            else
                OnTradeOperationComplete(tr, "FAILED to enter TRADE position: ");
        }

        protected void OnTradeOperationComplete(TradeResult tr, string errorMsg)
        {
            if (!tr.IsSuccessful)
            {
                if (tr.Position != null)
                    Print(errorMsg + tr.Error, " Position: ", tr.Position.Label, " ", tr.Position.TradeType, " ", Time);
                if (tr.PendingOrder != null)
                    Print(errorMsg + tr.Error, " PendingOrder: ", tr.PendingOrder.Label, " ", tr.PendingOrder.TradeType, " ", Time);
            }
        }

        protected double calcTakeProfit(int orderCount)
        {
            double tp = 0;

            tp = MinTakeProfit * (1 / Symbol.TickSize) + orderCount * TPSpacing;

            if (tp > MaxTakeProfit * (1 / Symbol.TickSize))
                tp = MaxTakeProfit * (1 / Symbol.TickSize);

            return tp;
        }

        protected void setCascadingTakeProfit()
        {
            IEnumerable<Position> orderedPositions = Positions.OrderBy(position => position.EntryPrice);

            int positionCount = 0;
            foreach (Position p in orderedPositions)
            {
                try
                {
                    if (isThisBotId(p.Label))
                    {
                        ModifyPositionAsync(p, p.StopLoss, p.EntryPrice - calcTakeProfit(positionCount), OnModifyTakeProfitComplete);
                    }
                } catch (Exception e)
                {
                    Print("Failed to Modify Position: " + e.Message);
                }
                positionCount++;
            }
        }

        protected void PositionsOnOpened(PositionOpenedEventArgs args)
        {
            if (isThisBotId(args.Position.Label))
            {
                _openedPositionsCount++;

                //Capture last Position Opened i.e. the furthest away
                _lastPositionTradeType = args.Position.TradeType;
                _lastPositionEntryPrice = args.Position.EntryPrice;
                _lastPositionLabel = args.Position.Label;

                //Set TP for all positions based on their entry point if Pending Orders are triggered
                if (Positions.Count > NumberOfPositions)
                    setCascadingTakeProfit();
            }

        }

        protected void PositionsOnClosed(PositionClosedEventArgs args)
        {
            if (isThisBotId(args.Position.Label))
            {
                _closedPositionsCount++;

                _dayProfitTotal += args.Position.GrossProfit;
                _dayPipsTotal += args.Position.Pips;

                debugCSV.Add(args.Position.Label + "," + args.Position.GrossProfit + "," + args.Position.Pips + "," + args.Position.EntryPrice + "," + History.FindLast(args.Position.Label, Symbol, args.Position.TradeType).ClosingPrice + "," + args.Position.StopLoss + "," + args.Position.TakeProfit + "," + Time.DayOfWeek + "," + Time + debugState());

                //Taking profit
                if (args.Position.GrossProfit > 0)
                {
                    //capture last position take profit price
                    setLastProfitPrice(args.Position.TradeType);

                    //capture last closed position entry price
                    _lastClosedPositionEntryPrice = args.Position.EntryPrice;

                    //Set trailing SL
                    _isTrailingStopsActive = true;
                }
            }
        }

        protected double calculateChaseFactor()
        {
            double percentClosed = 0;
            if (_openedPositionsCount > 0)
            {
                percentClosed = (_closedPositionsCount / _openedPositionsCount) * 100;
            }
            return percentClosed;
        }


        protected void setLastProfitPrice(TradeType lastProfitTradeType)
        {
            if (lastProfitTradeType == TradeType.Buy)
                _lastProfitPrice = Symbol.Ask;
            if (lastProfitTradeType == TradeType.Sell)
                _lastProfitPrice = Symbol.Bid;
        }

        protected bool IsTradingTime()
        {
            return _marketTimeInfo.IsPlacePendingOrdersTime(IsBacktesting, Server.Time);
        }

        protected bool IsTradingTimeIn(int mins)
        {
            return _marketTimeInfo.IsTimeBeforeOpen(IsBacktesting, Server.Time, mins);
        }

        protected void setAllStopLossesWithBuffer(double SLPrice)
        {
            switch (_lastPositionTradeType)
            {
                case TradeType.Buy:
                    setStopLossForAllPositions(SLPrice - HardStopLoss);
                    break;
                case TradeType.Sell:
                    setStopLossForAllPositions(SLPrice + HardStopLoss);
                    break;
            }
        }

        //Increase the volume based on Orders places and volume levels and multiplier until max volume reached
        protected int setVolume(int orderCount)
        {

            double orderVolumeLevel = orderCount / OrderVolumeLevels;
            double volume = VolumeMax / Math.Pow(VolumeMultipler, orderVolumeLevel);

            if (volume < Volume)
            {
                volume = Volume;
            }

            return (int)volume;
        }

        protected void CloseAllPositions()
        {
            //Close any outstanding pending orders
            foreach (Position p in Positions)
            {
                try
                {
                    if (isThisBotId(p.Label))
                    {
                        ClosePositionAsync(p, OnClosePositionComplete);
                    }
                } catch (Exception e)
                {
                    Print("Failed to Close Position: " + e.Message);
                }
            }
        }

        //Check whether a position or order is managed by this bot instance.
        protected bool isThisBotId(string label)
        {
            string id = label.Substring(0, 5);
            if (id.Equals(_botId))
                return true;
            else
                return false;
        }

        protected void ResetSwordFish()
        {
            if (_isReset)
                return;

            reportDay();

            //reset position counters
            _openedPositionsCount = 0;
            _closedPositionsCount = 0;
            _orderCountLabel = 0;

            //reset Last Position variables
            _lastPositionLabel = "NO LAST POSITION SET";
            _lastPositionEntryPrice = 0;
            _lastClosedPositionEntryPrice = 0;
            _lastProfitPrice = 0;

            //reset risk management variables
            _divideTrailingStopPips = 1;
            _isTrailingStopsActive = false;
            _isBreakEvenStopLossActive = false;
            _isHardSLActive = false;
            _isHardSLLastPositionEntryPrice = false;
            _isHardSLLastProfitPrice = false;

            // swordfish bot state variables
            _startPriceCaptured = false;
            _ordersPlaced = false;
            _isPendingOrdersClosed = false;
            _isTerminated = false;
            _isReset = true;
            _isReducedRiskTime = false;

            // reset reporting variables
            _dayProfitTotal = 0;
            _dayPipsTotal = 0;
            _spikePeakPips = 0;
            _spikePeakPrice = 0;
        }

        protected void reportDay()
        {
            string profit = "";
            if (_dayProfitTotal != 0 && _dayPipsTotal != 0)
            {
                profit = ("TOTALS," + _dayProfitTotal + "," + _dayPipsTotal + "," + _openedPositionsCount + "," + _spikePeakPips + "," + Time.DayOfWeek + "," + Time);
                debugCSV.Add("--------------------------------");
                debugCSV.Add(",Profit,Pips,Opened Positions,Spike Peak,Day,Date/Time,");
                debugCSV.Add(profit);
            }
        }



        protected string debugState()
        {

            string state = "";
            // Position counters
            state += "," + _openedPositionsCount;
            state += "," + _closedPositionsCount;

            // Last Position variables
            state += "," + _lastPositionEntryPrice;
            state += "," + _lastClosedPositionEntryPrice;
            state += "," + _lastProfitPrice;
            state += "," + _lastPositionLabel;

            // risk management variables
            state += "," + _divideTrailingStopPips;
            state += "," + _isTrailingStopsActive;
            state += "," + _isBreakEvenStopLossActive;
            state += "," + _isHardSLActive;
            state += "," + _isHardSLLastPositionEntryPrice;
            state += "," + _isHardSLLastProfitPrice;

            // swordfish bot state variables
            state += "," + _startPriceCaptured;
            state += "," + _ordersPlaced;
            state += "," + _isReset;
            state += "," + _isTerminated;
            state += "," + _isReducedRiskTime;

            return state;
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
            System.IO.File.WriteAllLines("C:\\Users\\alist\\Desktop\\Divi\\" + _marketTimeInfo.market + "-" + _botId + "-" + "Divi-" + getTimeStamp(true) + ".csv", debugCSV.ToArray());
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
                    _marketTimeInfo.market = "FTSE";
                    _marketTimeInfo.tz = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
                    // Market for swordfish trades opens at 8:00am.
                    _marketTimeInfo.open = new TimeSpan(16, 29, 0);
                    // Market for swordfish trades closes at 8:05am.
                    _marketTimeInfo.close = new TimeSpan(16, 31, 0);
                    // Close all open Swordfish position at 11:29am before US opens.
                    _marketTimeInfo.closeAll = new TimeSpan(18, 45, 0);

                    break;
                case "GER30":
                    _marketTimeInfo.market = "DAX";
                    _marketTimeInfo.tz = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                    // Market for swordfish opens at 9:00.
                    _marketTimeInfo.open = new TimeSpan(9, 0, 0);
                    // Market for swordfish closes at 9:05.
                    _marketTimeInfo.close = new TimeSpan(9, 3, 0);
                    // Close all open Swordfish position at 11:29am before US opens.
                    _marketTimeInfo.closeAll = new TimeSpan(11, 29, 0);
                    break;
                case "HK50":
                    _marketTimeInfo.market = "HSI";
                    _marketTimeInfo.tz = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
                    // Market for swordfish opens at 9:00.
                    _marketTimeInfo.open = new TimeSpan(9, 30, 0);
                    // Market for swordfish closes at 9:05.
                    _marketTimeInfo.close = new TimeSpan(9, 35, 0);
                    // Close all open Swordfish positions
                    _marketTimeInfo.closeAll = new TimeSpan(11, 30, 0);
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
    public bool IsPlacePendingOrdersTime(bool isBackTesting, DateTime serverTime)
    {
        if (isBackTesting)
        {
            return IsOpenAt(serverTime);
        }
        else
        {
            return IsOpenAt(DateTime.UtcNow);
        }
    }

    //Time during which Swordfish positions risk should be managed
    public bool IsReduceRiskTime(bool isBackTesting, DateTime serverTime, int reduceRiskTimeFromOpen)
    {
        if (isBackTesting)
        {
            return IsTimeAfterOpen(serverTime, reduceRiskTimeFromOpen);
        }
        else
        {
            return IsTimeAfterOpen(DateTime.UtcNow, reduceRiskTimeFromOpen);
        }
    }

    //Time from open
    public bool IsTimeBeforeOpen(bool isBackTesting, DateTime serverTime, int timeFromOpen)
    {
        if (isBackTesting)
        {
            return IsTimeBeforeOpen(serverTime, timeFromOpen);
        }
        else
        {
            return IsTimeBeforeOpen(DateTime.UtcNow, timeFromOpen);
        }
    }

    //Is the current time within the period Swordfish positions can remain open.
    public bool IsCloseAllPositionsTime(bool isBackTesting, DateTime serverTime)
    {

        if (isBackTesting)
        {
            return IsCloseAllAt(serverTime);
        }
        else
        {
            return IsCloseAllAt(DateTime.UtcNow);
        }
    }

    //Is the current time within the period Swordfish Pending Orders can be placed.
    private bool IsOpenAt(DateTime dateTimeUtc)
    {
        DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, tz);
        return (tzTime.TimeOfDay >= open & tzTime.TimeOfDay <= close);
    }

    //Is the current time after the time period when risk should be reduced.
    private bool IsTimeAfterOpen(DateTime dateTimeUtc, int timeFromOpen)
    {
        DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, tz);
        return (tzTime.TimeOfDay >= open.Add(TimeSpan.FromMinutes(timeFromOpen)));
    }

    //Is the current time after the time period when risk should be reduced.
    private bool IsTimeBeforeOpen(DateTime dateTimeUtc, int timeToOpen)
    {
        DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, tz);
        return (tzTime.TimeOfDay >= open.Subtract(TimeSpan.FromMinutes(timeToOpen)));
    }

    //Is the current time within the period Swordfish positions can remain open.
    private bool IsCloseAllAt(DateTime dateTimeUtc)
    {
        DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, tz);
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









