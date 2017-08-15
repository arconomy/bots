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

        [Parameter("Use Bollinger Bollinger Band Entry", DefaultValue = false)]
        public bool useBollingerBandEntry { get; set; }

        [Parameter("Pips inside Bollinger Band Entry", DefaultValue = 2)]
        public int targetBolliEntryPips { get; set; }

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
        public int retraceLevel1 { get; set; }

        [Parameter("Retrace level 2 Percentage", DefaultValue = 50)]
        public int retraceLevel2 { get; set; }

        [Parameter("Retrace level 3 Percentage", DefaultValue = 66)]
        public int retraceLevel3 { get; set; }

        [Parameter("Final order placed Hard SL", DefaultValue = 10)]
        public double FinalOrderStopLoss { get; set; }

        [Parameter("Triggered Hard SL buffer", DefaultValue = 20)]
        public double HardStopLossBuffer { get; set; }

        [Parameter("Trailing fixed SL", DefaultValue = 5)]
        public double TrailingStopPips { get; set; }

        protected MarketTimeInfo _swordFishTimeInfo;
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
        protected bool _isSwordfishTerminated = false;
        protected bool _isSwordFishReset = true;
        protected bool _isReducedRiskTime = false;
        protected string _botId = null;

        List<string> debugCSV = new List<string>();

        //Performance Reporting
        protected double _dayProfitTotal = 0;
        protected double _dayPipsTotal = 0;
        protected double _spikePeakPips = 0;
        protected double _spikePeakPrice = 0;

        protected int _count = 0;

        protected override void OnStart()
        {
            _botId = generateBotId();
            _swordFishTimeInfo = new MarketTimeInfo();
            setTimeZone();
            _boli = Indicators.BollingerBands(DataSeriesSource, 2, 20, MovingAverageType.Exponential);

            Positions.Opened += PositionsOnOpened;
            Positions.Closed += PositionsOnClosed;

            debugCSV.Add("PARAMETERS");
            debugCSV.Add("useBollingerBandEntry," + useBollingerBandEntry.ToString());
            debugCSV.Add("targetBolliEntryPips," + targetBolliEntryPips.ToString());
            debugCSV.Add("SwordFishTrigger," + SwordFishTrigger.ToString());
            debugCSV.Add("OrderEntryOffset," + OrderEntryOffset.ToString());
            debugCSV.Add("OrderSpacing," + OrderSpacing.ToString());
            debugCSV.Add("OrderSpacingLevels," + OrderSpacingLevels.ToString());
            debugCSV.Add("OrderSpacingMultipler," + OrderSpacingMultipler.ToString());
            debugCSV.Add("OrderSpacingMax," + OrderSpacingMax.ToString());
            debugCSV.Add("NumberOfOrders," + NumberOfOrders.ToString());
            debugCSV.Add("Volume," + Volume.ToString());
            debugCSV.Add("VolumeMax," + VolumeMax.ToString());
            debugCSV.Add("OrderVolumeLevels," + OrderVolumeLevels.ToString());
            debugCSV.Add("VolumeMultipler," + VolumeMultipler.ToString());
            debugCSV.Add("TakeProfit," + TakeProfit.ToString());
            debugCSV.Add("ReducePositionRiskTime," + ReducePositionRiskTime.ToString());
            debugCSV.Add("retraceEnabled," + retraceEnabled.ToString());
            debugCSV.Add("retraceLevel1," + retraceLevel1.ToString());
            debugCSV.Add("retraceLevel2," + retraceLevel2.ToString());
            debugCSV.Add("retraceLevel3," + retraceLevel3.ToString());
            debugCSV.Add("FinalOrderStopLoss," + FinalOrderStopLoss.ToString());
            debugCSV.Add("HardStopLossBuffer," + HardStopLossBuffer.ToString());
            debugCSV.Add("TrailingStopPips," + TrailingStopPips.ToString());
            debugCSV.Add("--------------------------");

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

            switch (_count)
            {

                case 0:
                    {
                        Print(Time);
                        break;
                    }
                case 10:
                    {
                        _count = 0;
                        break;
                    }
                default:
                    _count++;
                    break;
            }

            // If backtesting use the Server.Time.        
            if (IsSwordFishTime())
            {
                //Start Swordfishing
                if (_isSwordFishReset)
                    _isSwordFishReset = false;

                if (!_openPriceCaptured)
                {
                    //Get the Market Open Price
                    _openPrice = MarketSeries.Close.LastValue;
                    Print("OPEN PRICE: " + _openPrice);
                    _openPriceCaptured = true;
                }

                if (!_ordersPlaced)
                {
                    //Price moves 5pts UP from open then look to set SELL LimitOrders
                    if (_openPrice + SwordFishTrigger < Symbol.Bid)
                    {
                        placeSellLimitOrders();
                    }
                    //Price moves 5pts DOWN from open then look to set BUY LimitOrders
                    else if (_openPrice - SwordFishTrigger > Symbol.Ask)
                    {
                        placeBuyLimitOrders();
                    }
                }

                captureSpikePeak();
            }
            //It is outside SwordFish Time
            else
            {
                if (_ordersPlaced)
                {
                    if (_openedPositionsCount - _closedPositionsCount > 0)
                    {

                        //If positions were opened at the end of swordfish time but have not recorded a spike peak
                        if (_spikePeakPips == 0)
                            captureSpikePeak();

                        //Look to reduce risk as Spike retraces
                        ManagePositionRisk();

                        //Positions still open after ReducePositionRiskTime
                        if (!_isReducedRiskTime && _swordFishTimeInfo.IsReduceRiskTime(IsBacktesting, Server.Time, ReducePositionRiskTime))
                        {
                            //Reduce Trailing Stop Loss by 50%
                            // DivideTrailingStopPips = 2;
                            _isReducedRiskTime = true;
                        }

                        //If trades still open at ClosingAllTime then take the hit and close remaining positions
                        if (!_isSwordfishTerminated && _swordFishTimeInfo.IsCloseAllPositionsTime(IsBacktesting, Server.Time))
                        {
                            CloseAllPositions();
                            _isSwordfishTerminated = true;
                        }
                    }
                    else
                    {
                        //No positions opened and out of Swordfish time
                        if (!_isPendingOrdersClosed)
                            CloseAllPendingOrders();
                        ResetSwordFish();
                    }

                    //Out of Swordfish time and all positions that were opened are now closed
                    if (_openedPositionsCount > 0 && _openedPositionsCount - _closedPositionsCount == 0)
                        ResetSwordFish();
                }
                //No Orders were placed and it is out of swordfish time therefore reset Swordfish
                else
                {
                    ResetSwordFish();
                }
            }

            // If Trailing stop is active update position SL's - Remove TP as trailing position.
            if (_isTrailingStopsActive)
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
                                    ModifyPositionAsync(p, newStopLossPrice, null, onTradeOperationComplete);
                                }
                            }
                        } catch (Exception e)
                        {
                            Print("Failed to Modify Position:" + e.Message);
                        }
                    }
                }
            }
        }


        protected void captureSpikePeak()
        {
            //Capture the highest point of the Spike within Swordfish Time
            if (_openedPositionsCount > 0)
            {
                switch (_lastPositionTradeType)
                {
                    case TradeType.Buy:
                        {
                            //If we are buying then spike is down so look for prices less than current spikePeakPrice
                            if (Symbol.Bid < _spikePeakPrice || _spikePeakPrice == 0)
                            {
                                _spikePeakPrice = Symbol.Bid;
                                _spikePeakPips = _openPrice - Symbol.Bid;
                            }
                            break;
                        }
                    case TradeType.Sell:
                        {
                            //If we are selling then spike is up so look for prices more than current spikePeakPrice
                            if (Symbol.Ask > _spikePeakPrice || _spikePeakPrice == 0)
                            {
                                _spikePeakPrice = Symbol.Ask;
                                _spikePeakPips = Symbol.Ask - _openPrice;
                            }

                            break;
                        }
                }
            }
        }

        protected void setBreakEvens(double breakEvenTriggerPrice)
        {
            foreach (Position p in Positions)
            {
                try
                {
                    if (isThisBotId(p.Label))
                    {
                        if (_lastPositionTradeType == TradeType.Buy)
                        {
                            if (breakEvenTriggerPrice > p.EntryPrice)
                            {
                                ModifyPositionAsync(p, p.EntryPrice + HardStopLossBuffer, p.TakeProfit, onTradeOperationComplete);
                            }
                        }

                        if (_lastPositionTradeType == TradeType.Sell)
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

        //Place Buy Limit Orders
        protected void placeBuyLimitOrders()
        {
            //Place Buy Limit Orders
            for (int OrderCount = 0; OrderCount < NumberOfOrders; OrderCount++)
            {
                try
                {
                    tradeData data = new tradeData 
                    {
                        tradeType = TradeType.Buy,
                        symbol = Symbol,
                        volume = setVolume(OrderCount),
                        entryPrice = calcBuyEntryPrice(OrderCount),
                        label = _botId + "-" + getTimeStamp() + _swordFishTimeInfo.market + "-SWF#" + OrderCount,
                        stopLossPips = setPendingOrderStopLossPips(OrderCount, NumberOfOrders),
                        takeProfitPips = TakeProfit * (1 / Symbol.TickSize)
                    };

                    if (data == null)
                        continue;

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
            }

            //All Buy Limit Orders have been placed
            _ordersPlaced = true;
        }

        protected double calcBuyEntryPrice(int orderCount)
        {
            //OPTIONAL - Bollinger band indicates whether market is oversold or over bought.
            if (useBollingerBandEntry)
            {
                //Use Bolinger Band limit as first order entry point.
                return _boli.Bottom.Last(0) + targetBolliEntryPips - calcOrderSpacingDistance(orderCount);
            }
            else
            {
                return _openPrice - OrderEntryOffset - calcOrderSpacingDistance(orderCount);
            }
        }

        //Returns the distance from the first entry point to an order based on OrderSpacingMultipler and OrderMultiplierLevels until OrderSpacingMax reached
        protected int calcOrderSpacingDistance(int orderCount)
        {
            double orderSpacingLevel = 0;
            double orderSpacing = 0;
            double orderSpacingResult = 0;

            for (int i = 1; i <= orderCount; i++)
            {
                orderSpacingLevel = i / OrderSpacingLevels;
                orderSpacing = Math.Pow(OrderSpacingMultipler, orderSpacingLevel) * OrderSpacing;

                if (orderSpacing > OrderSpacingMax)
                {
                    orderSpacing = OrderSpacingMax;
                }

                orderSpacingResult += orderSpacing;
            }

            return (int)orderSpacingResult;
        }


        // Place Sell Limit Orders
        protected void placeSellLimitOrders()
        {
            //Place Sell Limit Orders
            for (int OrderCount = 0; OrderCount < NumberOfOrders; OrderCount++)
            {
                try
                {
                    tradeData data = new tradeData 
                    {
                        tradeType = TradeType.Sell,
                        symbol = Symbol,
                        volume = setVolume(OrderCount),
                        entryPrice = calcSellEntryPrice(OrderCount),
                        label = _botId + "-" + getTimeStamp() + _swordFishTimeInfo.market + "-SWF#" + OrderCount,
                        stopLossPips = setPendingOrderStopLossPips(OrderCount, NumberOfOrders),
                        takeProfitPips = TakeProfit * (1 / Symbol.TickSize)
                    };
                    if (data == null)
                        continue;

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
            }


            //All Sell Limit Orders have been placed
            _ordersPlaced = true;
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
                return _boli.Top.Last(0) - targetBolliEntryPips + calcOrderSpacingDistance(orderCount);
            }
            else
            {
                return _openPrice + OrderEntryOffset + calcOrderSpacingDistance(orderCount);
            }
        }


        //Calculate a new orderCount number for when tick jumps
        protected int calculateNewOrderCount(int orderCount, double currentTickPrice)
        {
            double tickJumpIntoRange = Math.Abs(_openPrice - currentTickPrice) - OrderEntryOffset;
            double pendingOrderRange = calcOrderSpacingDistance(NumberOfOrders);
            double pendingOrdersPercentageJumped = tickJumpIntoRange / pendingOrderRange;
            double newOrderCount = NumberOfOrders * pendingOrdersPercentageJumped;

            if (newOrderCount > orderCount)
                return (int)newOrderCount;
            else
                return (int)orderCount;
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
            }

        }

        protected void PositionsOnClosed(PositionClosedEventArgs args)
        {
            if (isThisBotId(args.Position.Label))
            {
                _closedPositionsCount++;

                _dayProfitTotal += args.Position.GrossProfit;
                _dayPipsTotal += args.Position.Pips;
                debugCSV.Add("TRADE," + args.Position.GrossProfit + "," + args.Position.Pips + "," + Time.DayOfWeek + "," + args.Position.Label + "," + args.Position.EntryPrice + "," + History.FindLast(args.Position.Label, Symbol, args.Position.TradeType).ClosingPrice + "," + args.Position.StopLoss + "," + args.Position.TakeProfit + "," + Time + debugState());

                //Last position's SL has been triggered for a loss - NOT a swordfish
                if (_lastPositionLabel == args.Position.Label && args.Position.GrossProfit < 0)
                {
                    Print("CLOSING ALL POSITIONS due to furthest position losing");
                    CloseAllPendingOrders();
                    CloseAllPositions();
                    _isSwordfishTerminated = true;
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
        }

        protected void ManagePositionRisk()
        {

            if (retraceEnabled)
            {
                //Calculate spike retrace factor
                double retraceFactor = calculateRetraceFactor();

                if (_isReducedRiskTime)
                {
                    //Reduce all retrace limits
                    retraceLevel1 = retraceLevel1 / 2;
                    retraceLevel2 = retraceLevel2 / 2;
                    retraceLevel3 = retraceLevel3 / 2;
                }

                //Set hard stop losses as soon as Swordfish time is over
                if (!_isHardSLLastPositionEntryPrice && !IsSwordFishTime())
                {
                    //Close any positions that have not been triggered
                    if (!_isPendingOrdersClosed)
                        CloseAllPendingOrders();

                    setAllStopLosses(_lastPositionEntryPrice);
                    _isHardSLLastPositionEntryPrice = true;
                }

                //Set hard stop losses and activate Trail if Spike has retraced between than retraceLevel1 and retraceLevel2
                if (_isReducedRiskTime || (retraceLevel2 > retraceFactor && retraceFactor > retraceLevel1))
                {
                    //Close any positions that have not been triggered
                    if (!_isPendingOrdersClosed)
                        CloseAllPendingOrders();

                    //If Hard SL has not been set yet
                    if (!_isHardSLLastPositionEntryPrice && _lastPositionEntryPrice > 0)
                    {
                        setAllStopLosses(_lastPositionEntryPrice);
                        _isHardSLLastPositionEntryPrice = true;
                    }
                    //Active Breakeven Stop Losses
                    _isBreakEvenStopLossActive = true;
                }

                //Set harder SL and active BreakEven if it has retraced between than retraceLevel2 and retraceLevel3
                if (_isReducedRiskTime || (retraceLevel3 > retraceFactor && retraceFactor > retraceLevel2))
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
                if (_isReducedRiskTime || retraceFactor > retraceLevel3)
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
            if (_lastPositionTradeType == TradeType.Sell)
            {
                //Position are Selling
                percentRetrace = (Symbol.Bid - _openPrice) / (_lastPositionEntryPrice - _openPrice);
            }

            if (_lastPositionTradeType == TradeType.Buy)
            {
                //Positions are buying
                percentRetrace = (_openPrice - Symbol.Ask) / (_openPrice - _lastPositionEntryPrice);
            }

            percentRetrace = 1 - percentRetrace;
            percentRetrace = percentRetrace * 100;

            return percentRetrace;
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


        protected void setLastProfitPrice(TradeType lastProfitTradeType)
        {
            if (lastProfitTradeType == TradeType.Buy)
                _lastProfitPrice = Symbol.Ask;
            if (lastProfitTradeType == TradeType.Sell)
                _lastProfitPrice = Symbol.Bid;
        }

        protected bool IsSwordFishTime()
        {
            return _swordFishTimeInfo.IsPlacePendingOrdersTime(IsBacktesting, Server.Time);
        }

        protected void setAllStopLosses(double SLPrice)
        {
            switch (_lastPositionTradeType)
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

        //Increase the volume based on Orders places and volume levels and multiplier until max volume reached
        protected int setVolume(int orderCount)
        {

            double orderVolumeLevel = orderCount / OrderVolumeLevels;
            double volume = Math.Pow(VolumeMultipler, orderVolumeLevel) * Volume;

            if (volume > VolumeMax)
            {
                volume = VolumeMax;
            }

            return (int)volume;
        }

        protected void CloseAllPendingOrders()
        {
            //Close any outstanding pending orders
            foreach (PendingOrder po in PendingOrders)
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
            }
            _isPendingOrdersClosed = true;
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
                        ClosePositionAsync(p, onTradeOperationComplete);
                    }
                } catch (Exception e)
                {
                    Print("Failed to Close Position: " + e.Message);
                }
            }
        }

        protected void setStopLossForAllPositions(double stopLossPrice)
        {
            foreach (Position p in Positions)
            {
                try
                {
                    if (isThisBotId(p.Label))
                    {
                        ModifyPositionAsync(p, stopLossPrice, p.TakeProfit, onTradeOperationComplete);
                    }
                } catch (Exception e)
                {
                    Print("Failed to Modify Position: " + e.Message);
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
            if (_isSwordFishReset)
                return;

            reportDay();

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
            _isSwordfishTerminated = false;
            _isSwordFishReset = true;
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
                profit = ("DAY TOTAL," + _dayProfitTotal + "," + _dayPipsTotal + "," + _openedPositionsCount + "," + _spikePeakPips + "," + Time.DayOfWeek + "," + Time);
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
            state += "," + _isHardSLLastClosedPositionEntryPrice;
            state += "," + _isHardSLLastPositionEntryPrice;
            state += "," + _isHardSLLastProfitPrice;

            // swordfish bot state variables
            state += "," + _isHardSLLastProfitPrice;
            state += "," + _ordersPlaced;
            state += "," + _isSwordFishReset;
            state += "," + _isSwordfishTerminated;
            state += "," + _isReducedRiskTime;

            return state;
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
            System.IO.File.WriteAllLines("C:\\Users\\alist\\Desktop\\swordfish\\" + _swordFishTimeInfo.market + "-" + _botId + "-" + "swordfish-" + getTimeStamp(true) + ".csv", debugCSV.ToArray());
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
                    _swordFishTimeInfo.market = "FTSE";
                    _swordFishTimeInfo.tz = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
                    // Market for swordfish trades opens at 8:00am.
                    _swordFishTimeInfo.open = new TimeSpan(7, 59, 50);
                    // Market for swordfish trades closes at 8:05am.
                    _swordFishTimeInfo.close = new TimeSpan(8, 5, 0);
                    // Close all open Swordfish position at 11:29am before US opens.
                    _swordFishTimeInfo.closeAll = new TimeSpan(11, 29, 0);

                    break;
                case "GER30":
                    _swordFishTimeInfo.market = "DAX";
                    _swordFishTimeInfo.tz = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                    // Market for swordfish opens at 9:00.
                    _swordFishTimeInfo.open = new TimeSpan(8, 59, 50);
                    // Market for swordfish closes at 9:05.
                    _swordFishTimeInfo.close = new TimeSpan(9, 3, 0);
                    // Close all open Swordfish position at 11:29am before US opens.
                    _swordFishTimeInfo.closeAll = new TimeSpan(11, 29, 0);
                    break;
                case "HK50":
                    _swordFishTimeInfo.market = "HSI";
                    _swordFishTimeInfo.tz = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
                    // Market for swordfish opens at 9:00.
                    _swordFishTimeInfo.open = new TimeSpan(9, 30, 0);
                    // Market for swordfish closes at 9:05.
                    _swordFishTimeInfo.close = new TimeSpan(9, 35, 0);
                    // Close all open Swordfish positions
                    _swordFishTimeInfo.closeAll = new TimeSpan(11, 30, 0);
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
        DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, tz);
        return (tzTime.TimeOfDay >= open & tzTime.TimeOfDay <= close);
    }

    //Is the current time after the time period when risk should be reduced.
    public bool IsReduceRiskAt(DateTime dateTimeUtc, int reduceRiskTimeFromOpen)
    {
        DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, tz);
        return (tzTime.TimeOfDay >= open.Add(TimeSpan.FromMinutes(reduceRiskTimeFromOpen)));
    }

    //Is the current time within the period Swordfish positions can remain open.
    public bool IsCloseAllAt(DateTime dateTimeUtc)
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









