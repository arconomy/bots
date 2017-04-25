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

        [Parameter("Breakout duration period", DefaultValue = 45)]
        public int _breakOutTimePeriod { get; set; }
        
        [Parameter("Mins after Breakout to reduce position risk", DefaultValue = 45)]
        public int _reducePositionRiskTime { get; set; }

        [Parameter("Enable Chase risk management", DefaultValue = true)]
        public bool _isChaseEnabled { get; set; }

        [Parameter("Chase level 1 Percentage", DefaultValue = 33)]
        public int _chaseLevel1 { get; set; }

        [Parameter("Chase level 2 Percentage", DefaultValue = 50)]
        public int _chaseLevel2 { get; set; }

        [Parameter("Chase level 3 Percentage", DefaultValue = 66)]
        public int _chaseLevel3 { get; set; }

        [Parameter("Initial Hard SL for last Order placed", DefaultValue = 5)]
        public double _firstOrderStopLoss { get; set; }

        [Parameter("Triggered Hard SL buffer", DefaultValue = 20)]
        public double _hardStopLossBuffer { get; set; }

        [Parameter("Trailing SL fixed distance", DefaultValue = 5)]
        public double _trailingStopPips { get; set; }

        //Price and Position Variables
        protected double _lastMidPrice;
        protected string _lastPositionLabel;
        protected TradeType _lastPositionTradeType;
        protected double _lastPositionEntryPrice;
        protected string _firstPositionLabel;
        protected TradeType _firstPositionTradeType;
        protected double _firstPositionEntryPrice;
        protected double _lastClosedPositionEntryPrice;
        protected double _lastProfitPrice;

        protected double _openedPositionsCount = 0;
        protected double _closedPositionsCount = 0;
        protected double _sellStopOrdersCount = 0;
        protected double _buyStopOrdersCount = 0;
        
        //Stop Loss Variables
        protected double _divideTrailingStopPips = 1;
        protected bool _isTrailingStopsActive = false;
        protected bool _isBreakEvenStopLossActive = false;
        protected bool _isHardSLFirstPositionEntryPrice = false;
        protected bool _isHardSLLastProfitPrice = false;
        protected bool _isHardSLLastClosedPositionEntryPrice = false;
       
        //BreakOut State Variables
        protected bool _isPendingBuyStopOrdersClosed = false;
        protected bool _isPendingSellStopOrdersClosed = false;
        protected bool _firstPositionCaptured = false;
        protected bool _buyOrdersPlaced = false;
        protected bool _sellOrdersPlaced = false;
        
        protected bool _isbreakOutPeriodSet = false;
        protected bool _isBreakOutReset = true;
        protected bool _isBreakOutTerminated = false;
        protected bool _isBreakOutActive = false;
        protected bool _isReducedRiskTime = false;

        //Market and bot identifiers
        protected MarketTimeInfo _majorsBotTimeInfo;
        protected string _botId = null;

        List<string> _debugCSV = new List<string>();

        //Performance Reporting
        protected double _profitTotal = 0;
        protected double _pipsTotal = 0;

        protected override void OnStart()
        {
            _botId = generateBotId();
            _majorsBotTimeInfo = new MarketTimeInfo();
            Positions.Opened += PositionsOnOpened;
            Positions.Closed += PositionsOnClosed;
            Timer.Start(_modifyPeriod);
        }

        protected string generateBotId()
        {
            Random randomIdGenerator = new Random();
            int id = randomIdGenerator.Next(0, 99999);
            return id.ToString("00000");
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

        protected override void OnTick()
        {
            // If there is a breakout then manage position
            if (_isBreakOutActive)
            {
                managePositionRisk();

                // If Trailing stop is active update position SL's
                if (_isTrailingStopsActive)
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
                                        ModifyPositionAsync(p, newStopLossPrice, null, onTradeOperationError);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Print("Failed to Modify Position:" + e.Message);
                            }

                        },
                        p));
                    }
                    Task.WaitAll(taskList.ToArray<Task>());
                }

                //If BreakOutPeriod is over then all Pending Orders will have been cancelled - so if all the positions that were open are closed then breakout can be reset
                if(isBreakOutPeriodOver() && _closedPositionsCount == _openedPositionsCount)
                {
                    resetBreakOut();
                }
            }
        }

        protected void managePositionRisk()
        {

           //Is the breakout period over
            if(!_isbreakOutPeriodSet)
            {
                _majorsBotTimeInfo.setbreakOut(IsBacktesting,Server.Time);
                _isbreakOutPeriodSet = true;
            }

            //Ride the lightening until Breakout period is over
            if (isBreakOutPeriodOver())
            {

                //Close any orders that are still open
                closeAllPendingOrders(_lastPositionTradeType);

                //Set hard stop losses as soon as BreakOut time is over
                if (!_isHardSLFirstPositionEntryPrice)
                {
                    setAllStopLosses(_firstPositionEntryPrice);
                    _isHardSLFirstPositionEntryPrice = true;
                }
                return;
            }

            if (_isChaseEnabled)
            {
                //Calculate chase factor
                double chaseFactor = calculateChaseFactor();

                if (isReduceRiskTime() || (_chaseLevel2 > chaseFactor && chaseFactor > _chaseLevel1))
                {
                    //If Hard SL has not been set yet
                    if (!_isHardSLFirstPositionEntryPrice)
                    {
                        setAllStopLosses(_firstPositionEntryPrice);
                        _isHardSLFirstPositionEntryPrice = true;
                    }
                    //Active Breakeven Stop Losses
                    _isBreakEvenStopLossActive = true;

                }

                if (isReduceRiskTime() || (_chaseLevel3 > chaseFactor && chaseFactor > _chaseLevel2))
                {
                    //Set hard stop losses
                    if (!_isHardSLLastClosedPositionEntryPrice && _lastClosedPositionEntryPrice > 0)
                    {
                        setAllStopLosses(_lastClosedPositionEntryPrice);
                        _isHardSLLastClosedPositionEntryPrice = true;
                    }
                    //Activate Trailing Stop Losses
                    _isTrailingStopsActive = true;
                }

                //Set hardest SL if Spike retraced past retraceLevel3
                if (isReduceRiskTime() || chaseFactor > _chaseLevel3)
                {
                    //Set hard stop losses
                    if (!_isHardSLLastProfitPrice && _lastProfitPrice > 0)
                    {
                        setAllStopLosses(_lastProfitPrice);
                        _isHardSLLastProfitPrice = true;
                    }
                }
            }

        }

        protected void setAllStopLosses(double SLPrice)
        {
            switch (_firstPositionTradeType)
            {
                case TradeType.Buy:
                    setStopLossForAllPositions(SLPrice - _hardStopLossBuffer);
                    break;
                case TradeType.Sell:
                    setStopLossForAllPositions(SLPrice + _hardStopLossBuffer);
                    break;
            }
        }


        //Return the percentage of closed positions or expected distance to chase (if not all orders were placed). 
        protected double calculateChaseFactor()
        {
            double chaseFactor = 0;
            double percentClosed = calculatePercentageClosed();
            double percentChased = calculatePercentageChased();
            if (percentClosed <= percentChased)
            {
                chaseFactor = percentChased;
            }
            else
            {
                chaseFactor = percentClosed;
            }
            return chaseFactor;
        }

        protected double calculatePercentageClosed()
        {
            double percentClosed = 0;
            if (_openedPositionsCount > 0)
            {
                percentClosed = (_closedPositionsCount / _openedPositionsCount) * 100;
            }
            return percentClosed;
        }

        protected double calculatePercentageChased()
        {
            double percentChased = 0;
            double chaseDistance = _numberOfOrders * _orderSpacing + _takeProfit;
            if (_lastPositionTradeType == TradeType.Sell)
            {
                //Positions are Selling
                percentChased = (Symbol.Bid - _firstPositionEntryPrice) / chaseDistance;
            }

            if (_lastPositionTradeType == TradeType.Buy)
            {
                //Positions are buying
                percentChased = (_firstPositionEntryPrice - Symbol.Ask) / chaseDistance;
            }

            percentChased = 1 - percentChased;
            percentChased = percentChased * 100;

            return percentChased;
        }

        //Check if breakout period is over
        protected bool isBreakOutPeriodOver()
        {
            return _majorsBotTimeInfo.IsReduceRiskTime(IsBacktesting, Server.Time,_breakOutTimePeriod);
        }

        //Check if breakout period is over
        protected bool isReduceRiskTime()
        {
            return _majorsBotTimeInfo.IsReduceRiskTime(IsBacktesting, Server.Time, _reducePositionRiskTime);
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
                newStopLossPrice = Symbol.Ask - _trailingStopPips / _divideTrailingStopPips;
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
                newStopLossPrice = Symbol.Bid + _trailingStopPips / _divideTrailingStopPips;
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

        protected override void OnTimer()
        {

            //Get the latest price
            _lastMidPrice = Symbol.Ask + Symbol.Spread / 2;

            //check status
            if (!_isBreakOutActive)
            {
                if (_isBreakOutReset)
                {
                    // Create new Pending Stop Orders
                    placeSellStopOrders();
                    placeBuyStopOrders();
                    _isBreakOutReset = false;
                }
                else
                {
                    modifyPendingOrders();
                }
            }
        }

        
        // Place Buy Stop Orders
        protected void placeBuyStopOrders()
        {
            //Place Sell Stop Orders
            List<Task> taskList = new List<Task>();
            for (int OrderCount = 0; OrderCount < _numberOfOrders; OrderCount++)
            {
                taskList.Add(Task.Factory.StartNew((Object obj) =>
                {
                    try
                    {
                        tradeData data = obj as tradeData;
                        if (data == null)
                            return;

                        //Check that entry price is valid
                        if (data.entryPrice > Symbol.Bid)
                        {
                            PlaceStopOrderAsync(data.tradeType, data.symbol, data.volume, data.entryPrice, data.label, data.stopLossPips, data.takeProfitPips, onBuyStopTradeOperationComplete);
                        }
                        else
                        {
                            //Tick price has 'jumped' - therefore avoid placing all PendingOrders by re-calculating the OrderCount to the equivelant entry point.
                            OrderCount = calculateNewOrderCount(OrderCount, Symbol.Ask);
                            ExecuteMarketOrderAsync(data.tradeType, data.symbol, data.volume, data.label + "X", data.stopLossPips, data.takeProfitPips, onTradeOperationError);
                        }
                    }
                    catch (Exception e)
                    {
                        Print("Failed to place Buy Stop Order: " + e.Message);
                    }

                },
                new tradeData()
                {
                    tradeType = TradeType.Buy,
                    symbol = Symbol,
                    volume = setVolume(OrderCount, _numberOfOrders),
                    entryPrice = calcBuyEntryPrice(OrderCount),
                    label = _botId + "-" + getTimeStamp() + _majorsBotTimeInfo._market + "-MJ-BUY#" + OrderCount,
                    stopLossPips = setPendingOrderStopLossPips(OrderCount, _numberOfOrders),
                    takeProfitPips = _takeProfit * (1 / Symbol.TickSize)
                }));
            }
            Task.WaitAll(taskList.ToArray<Task>());

            // Sell Stop Orders have been placed
            if(_sellStopOrdersCount>0)
                _sellOrdersPlaced = true;
        }
      
        protected double calcBuyEntryPrice(int orderCount)
        {
                return Symbol.Ask + _orderEntryOffset + orderCount * _orderSpacing;
        }

        // Place Sell Stop Orders
        protected void placeSellStopOrders()
        {
            //Place Sell Stop Orders
            List<Task> taskList = new List<Task>();
            for (int OrderCount = 0; OrderCount < _numberOfOrders; OrderCount++)
            {
                taskList.Add(Task.Factory.StartNew((Object obj) =>
                {
                    try
                    {
                        tradeData data = obj as tradeData;
                        if (data == null)
                            return;

                        //Check that entry price is valid
                        if (data.entryPrice < Symbol.Ask)
                        {
                            PlaceStopOrderAsync(data.tradeType, data.symbol, data.volume, data.entryPrice, data.label, data.stopLossPips, data.takeProfitPips, onSellStopTradeOperationComplete);
                        }
                        else
                        {
                            //Tick price has 'jumped' - therefore avoid placing all PendingOrders by re-calculating the OrderCount to the equivelant entry point.
                            OrderCount = calculateNewOrderCount(OrderCount, Symbol.Ask);
                            ExecuteMarketOrderAsync(data.tradeType, data.symbol, data.volume, data.label + "X", data.stopLossPips, data.takeProfitPips, onTradeOperationError);
                        }
                    }
                    catch (Exception e)
                    {
                        Print("Failed to place Sell Stop Order: " + e.Message);
                    }

                },
                new tradeData()
                {
                    tradeType = TradeType.Sell,
                    symbol = Symbol,
                    volume = setVolume(OrderCount, _numberOfOrders),
                    entryPrice = calcSellEntryPrice(OrderCount),
                    label = _botId + "-" + getTimeStamp() + _majorsBotTimeInfo._market + "-MJ-SELL#" + OrderCount,
                    stopLossPips = setPendingOrderStopLossPips(OrderCount, _numberOfOrders),
                    takeProfitPips = _takeProfit * (1 / Symbol.TickSize)
                }));
            }
            Task.WaitAll(taskList.ToArray<Task>());

            // Sell Stop Orders have been placed
            if(_sellStopOrdersCount>0)
                _sellOrdersPlaced = true;
        }

        protected void onTradeOperationError(TradeResult tr)
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

        //Keep a count of the number of Sell Stop Orders successfully placed
        protected void onSellStopTradeOperationComplete(TradeResult tr)
        { 
            if (tr.IsSuccessful)
            {
                _sellStopOrdersCount++;
            }
            else
            {
                onTradeOperationError(tr);
            }
        }

        //Keep a count of the number of Buy Stop Orders successfully placed
        protected void onBuyStopTradeOperationComplete(TradeResult tr)
        {
            if (tr.IsSuccessful)
            {
                _buyStopOrdersCount++;
            }
            else
            {
                onTradeOperationError(tr);
            }
        }

         protected double calcSellEntryPrice(int orderCount)
        {
            return Symbol.Bid - _entryOffset - orderCount * _orderSpacing;
        }

         //Calculate a new orderCount number for when tick jumps
         protected int calculateNewOrderCount(int _orderCount, double _currentTickPrice)
         {
             double tickJumpIntoRange = Math.Abs(_lastMidPrice - _currentTickPrice) - _orderEntryOffset;
             double pendingOrderRange = _numberOfOrders * _orderSpacing;
             double pendingOrdersPercentageJumped = tickJumpIntoRange / pendingOrderRange;
             double _newOrderCount = _numberOfOrders * pendingOrdersPercentageJumped;

             if (_newOrderCount > _orderCount)
                 return (int)_newOrderCount;
             else
                 return (int)_orderCount;
         }

        protected void modifyPendingOrders()
        {
            List<Task> taskList = new List<Task>();
            foreach (PendingOrder p in PendingOrders)
            {
                taskList.Add(Task.Factory.StartNew((Object obj) =>
                {
                    try
                    {
                        if (isThisBotId(p.Label))
                        {
                            if (p.TradeType == TradeType.Buy)
                            {
                                ModifyPendingOrderAsync(p, calcBuyEntryPrice(getOrderNumberFromLabel(p.Label)), p.StopLossPips, p.TakeProfitPips, null, onTradeOperationError);
                            }

                            if (p.TradeType == TradeType.Sell)
                            {
                                ModifyPendingOrderAsync(p, calcSellEntryPrice(getOrderNumberFromLabel(p.Label)), p.StopLossPips, p.TakeProfitPips, null, onTradeOperationError);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Print("Failed to Modify " + p.TradeType + " Stop Order " + p.Label + ": " + e.Message);
                    }

                },
                p));
            }
            Task.WaitAll(taskList.ToArray<Task>());
        }


        protected int getOrderNumberFromLabel(String label)
        {
            int orderCount;
            string tmplabel = label.Substring(label.IndexOf('#',0), 2);
            if(!int.TryParse(tmplabel,out orderCount))
            {
                tmplabel = tmplabel.Substring(0, 1);
                if (!int.TryParse(tmplabel, out orderCount))
                {
                    Print("Could not get order number from label: " + label);
                    orderCount = 0;
                }   
            }
            return orderCount;
        }


        protected void PositionsOnOpened(PositionOpenedEventArgs args)
        {
            if (isThisBotId(args.Position.Label))
            {
                _openedPositionsCount++;

                // Once a position is open set flag to manage the Breakout
                _isBreakOutActive = true;

                //Capture first Position Opened
                if (_firstPositionCaptured)
                {
                    _firstPositionTradeType = args.Position.TradeType;
                    _firstPositionEntryPrice = args.Position.EntryPrice;
                    _firstPositionLabel = args.Position.Label;
                    _firstPositionCaptured = true;
                }

                //Capture last Position Opened i.e. the furthest from profit and closest to current tick price
                _lastPositionTradeType = args.Position.TradeType;
                _lastPositionEntryPrice = args.Position.EntryPrice;
                _lastPositionLabel = args.Position.Label;
            }
        }

        protected void PositionsOnClosed(PositionClosedEventArgs args)
        {
            if (isThisBotId(args.Position.Label))
            {
                _closedPositionsCount++;

                debugTrade(args.Position);

                // first position's SL has been triggered for a loss - NOT breakout anymore
                if (_firstPositionLabel == args.Position.Label && args.Position.GrossProfit < 0)
                {
                    Print("CLOSING ALL POSITIONS due to first position losing");
                    closeAllPendingOrders(_firstPositionTradeType);
                    closeAllPositions();
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
                    managePositionRisk();

                    //BreakEven SL triggered in ManageRisk() function
                    if (_isBreakEvenStopLossActive)
                    {
                        setBreakEvens(_lastProfitPrice);
                    }
                }
            }
        }

        protected void closeAllPendingOrders(TradeType tradeType)
        {
            //Close any outstanding pending orders
            List<Task> taskList = new List<Task>();
            foreach (PendingOrder po in PendingOrders)
            {
                taskList.Add(Task.Factory.StartNew((Object obj) =>
                {
                    try
                    {
                        if (isThisBotId(po.Label) && po.TradeType == tradeType)
                        {
                            CancelPendingOrderAsync(po, onTradeOperationError);
                        }
                    }
                    catch (Exception e)
                    {
                        Print("Failed to Cancel Pending Order :" + e.Message);
                    }
                },
                po));
            }
            Task.WaitAll(taskList.ToArray<Task>());

            if(tradeType == TradeType.Buy)
               _isPendingBuyStopOrdersClosed = true;

            if(tradeType == TradeType.Sell)
                _isPendingSellStopOrdersClosed = true;
        }

        protected void setLastProfitPrice(TradeType lastProfitTradeType)
        {
            if (lastProfitTradeType == TradeType.Buy)
                _lastProfitPrice = Symbol.Ask;
            if (lastProfitTradeType == TradeType.Sell)
                _lastProfitPrice = Symbol.Bid;
        }

        protected void closeAllPositions()
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
                            ClosePositionAsync(p, onTradeOperationError);
                        }
                    }
                    catch (Exception e)
                    {
                        Print("Failed to Close Position: " + e.Message);
                    }
                },
                p));
            }
            Task.WaitAll(taskList.ToArray<Task>());
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

        protected string getTimeStamp(bool unformatted = false)
        {
            if (unformatted)
                return Time.Year.ToString() + Time.Month + Time.Day + Time.Minute + Time.Second;
            return Time.Year + "-" + Time.Month + "-" + Time.Day;
        }

       
        //Increase the volume based on Orders places and volume levels and multiplier until max volume reached
        protected int setVolume(int _orderCount, int _numberOfOrders)
        {
            double OrderVolumeLevel = _orderCount / _orderVolumeLevels;
            double Volume = Math.Pow(_volumeMultipler, OrderVolumeLevel) * _volume;

            if (Volume > _volumeMax)
            {
                Volume = _volumeMax;
            }
            return (int) Volume;
        }

        //Set a stop loss on the last Pending Order set to catch the break away train that never comes back!
        protected double setPendingOrderStopLossPips(int orderCount, int numberOfOrders)
        {
            if (orderCount == _numberOfOrders - 1)
            {
                return _firstOrderStopLoss * (1 / Symbol.TickSize);
            }
            else
            {
                return 0;
            }
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
                            ModifyPositionAsync(p, _stopLossPrice, p.TakeProfit, onTradeOperationError);
                        }
                    }
                    catch (Exception e)
                    {
                        Print("Failed to Modify Position: " + e.Message);
                    }
                },
               p));
            }
            Task.WaitAll(taskList.ToArray<Task>());
        }


        protected void debug(string msg, TradeResult tr)
        {
            if (tr.Position != null)
                Print(msg, " Position: ", tr.Position.Label, " ", tr.Position.TradeType, " ", Time);
            if (tr.PendingOrder != null)
                Print(msg, " Pending Order: ", tr.PendingOrder.Label, " ", tr.PendingOrder.TradeType, " ", Time);
        }

        protected void debugHeaders()
        {
            _debugCSV.Add("Item"
                + ",Profit"
                +",Pips"
                +",Day"
                +",Label"
                +",EntryPrice"
                +",ClosePrice"
                +",SL"
                +",TP"
                +",Date/Time"
                + ",_openedPositionsCount"
                +",_closedPositionsCount"
                +",_sellStopOrdersCount"
                +",_buyStopOrdersCount"
                +",_lastMidPrice"
                +",_lastPositionEntryPrice"
                +",_lastClosedPositionEntryPrice"
                +",_lastProfitPrice"
                +",_lastPositionLabel"
                +",_firstPositionEntryPrice"
                +",_firstPositionLabel"
                +",_divideTrailingStopPips"
                +",_isTrailingStopsActive"
                +",_isBreakEvenStopLossActive"
                +",_isHardSLLastClosedPositionEntryPrice"
                +",_isHardSLFirstPositionEntryPrice"
                +",_isHardSLLastProfitPrice"
                +",_isbreakOutPeriodSet"
                +",_isBreakOutActive"
                +",_buyOrdersPlaced"
                +",_sellOrdersPlaced"
                +",_isBreakOutReset"
                +",_isReducedRiskTime"
                +",_isPendingBuyStopOrdersClosed"
                +",_isPendingSellStopOrdersClosed"
                +",_firstPositionCaptured");
        }

        protected void debugTrade(Position p)
        {
            _profitTotal += p.GrossProfit;
            _pipsTotal += p.Pips;
            _debugCSV.Add("TRADE"
                + "," + p.GrossProfit
                + "," + p.Pips
                + "," + Time.DayOfWeek
                + "," + p.Label
                + "," + p.EntryPrice
                + "," + History.FindLast(p.Label, Symbol, p.TradeType).ClosingPrice
                + "," + p.StopLoss
                + "," + p.TakeProfit
                + "," + Time
                + debugState());
        }

        protected string debugState()
        {

            string state = "";

            // Position counters
            state += "," + _openedPositionsCount;
            state += "," + _closedPositionsCount;
            state += "," + _sellStopOrdersCount;
            state += "," + _buyStopOrdersCount;

            // Last Position variables
            state += "," + _lastMidPrice;
            state += "," + _lastPositionEntryPrice;
            state += "," + _lastClosedPositionEntryPrice;
            state += "," + _lastProfitPrice;
            state += "," + _lastPositionLabel;
            state += "," + _firstPositionEntryPrice;
            state += "," + _firstPositionLabel;

            // risk management variables
            state += "," + _divideTrailingStopPips;
            state += "," + _isTrailingStopsActive;
            state += "," + _isBreakEvenStopLossActive;
            state += "," + _isHardSLLastClosedPositionEntryPrice;
            state += "," + _isHardSLFirstPositionEntryPrice;
            state += "," + _isHardSLLastProfitPrice;

            // swordfish bot state variables
            state += "," + _isbreakOutPeriodSet;
            state += "," + _isBreakOutActive;
            state += "," + _buyOrdersPlaced;
            state += "," + _sellOrdersPlaced;
            state += "," + _isBreakOutReset;
            state += "," + _isReducedRiskTime;
            state += "," + _isPendingBuyStopOrdersClosed;
            state += "," + _isPendingSellStopOrdersClosed;
            state += "," + _firstPositionCaptured;

            return state;
        }


        protected void resetBreakOut()
        {
            if (_isBreakOutReset)
                return;
            
            //reset position counters
            _openedPositionsCount = 0;
            _closedPositionsCount = 0;
            _sellStopOrdersCount = 0;
            _buyStopOrdersCount = 0;
        
            //reset risk management variables
            _divideTrailingStopPips = 1;
            _isTrailingStopsActive = false;
            _isBreakEvenStopLossActive = false;
            _isHardSLFirstPositionEntryPrice = false;
            _isHardSLLastProfitPrice = false;
            _isHardSLLastClosedPositionEntryPrice = false;
       
            //reset BreakOut State Variables
            _isPendingBuyStopOrdersClosed = false;
            _isPendingSellStopOrdersClosed = false;
            _firstPositionCaptured = false;
            _buyOrdersPlaced = false;
            _sellOrdersPlaced = false;
        
            // bot state variables
            _isbreakOutPeriodSet = false;
            _isBreakOutReset = true;
            _isBreakOutTerminated = false;
            _isBreakOutActive = false;
            _isReducedRiskTime = false;

            //reset Position variables
            _lastPositionLabel = "NO LAST POSITION SET";
            _lastPositionEntryPrice = 0;
            _lastClosedPositionEntryPrice = 0;
            _lastProfitPrice = 0;

            _lastMidPrice = 0;
            _lastPositionLabel = "NO LAST POSITION SET";
            _lastPositionEntryPrice = 0;
            _firstPositionLabel = "NO LAST POSITION SET";
            _firstPositionEntryPrice = 0;
            _lastClosedPositionEntryPrice = 0;
            _lastProfitPrice = 0;

            string profit = "";
            if (_profitTotal != 0 && _pipsTotal != 0)
            {
                profit = ("BREAKOUT TOTAL," + _profitTotal + "," + _pipsTotal + "," + Time.DayOfWeek + "," + Time);
                _debugCSV.Add(profit);
            }

            _profitTotal = 0;
            _pipsTotal = 0;
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
            System.IO.File.WriteAllLines("C:\\Users\\alist\\Desktop\\major\\" + _majorsBotTimeInfo._market + "-" + _botId + "-" + "major-" + getTimeStamp(true) + ".csv", _debugCSV.ToArray());
        }
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


//Manage Market Opening Times
public struct MarketTimeInfo
{
    public String _market;
    public TimeZoneInfo _tz;
    public TimeSpan _open;
    public TimeSpan _close;
    public TimeSpan _closeAll;
    private TimeSpan _breakOutStart;

    public void setbreakOut(bool isBackTesting, DateTime serverTime)
    {
        if (isBackTesting)
        {
            _breakOutStart = serverTime.TimeOfDay;
        }
        else
        {
            _breakOutStart = DateTime.Now.TimeOfDay;
        }

    }


    //Time during which Swordfish positions risk should be managed
    public bool IsReduceRiskTime(bool isBackTesting, DateTime serverTime, int reduceRiskTimeFromOpen)
    {
        if (isBackTesting)
        {
            return IsReduceRiskAt(serverTime, reduceRiskTimeFromOpen);
        }
        else
        {
            return IsReduceRiskAt(DateTime.UtcNow, reduceRiskTimeFromOpen);
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
    public bool IsOpenAt(DateTime dateTimeUtc)
    {
        DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, _tz);
        return (tzTime.TimeOfDay >= _open & tzTime.TimeOfDay <= _close);
    }

    //Is the current time after the time period when risk should be reduced.
    public bool IsReduceRiskAt(DateTime dateTimeUtc, int reduceRiskTimeFromOpen)
    {
        DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, _tz);
        return (tzTime.TimeOfDay >= _breakOutStart.Add(TimeSpan.FromMinutes(reduceRiskTimeFromOpen)));
    }

    //Is the current time within the period Swordfish positions can remain open.
    public bool IsCloseAllAt(DateTime dateTimeUtc)
    {
        DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, _tz);
        return tzTime.TimeOfDay >= _closeAll;
    }
}



