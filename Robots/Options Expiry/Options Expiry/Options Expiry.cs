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

        [Parameter("# Pips from 10.10am price indicating a Spike", DefaultValue = 5)]
        public int SpikeIndicatorPips { get; set; }

        [Parameter("Close trading time mins after Start time", DefaultValue = 20)]
        public int CloseTime { get; set; }

        [Parameter("Reduce risk time mins after Start time", DefaultValue = 15)]
        public int ReducePositionRiskTime { get; set; }


        //Limit Orders Config
        [Parameter("Pips offset from 10.09am Price for 1st Limit Order", DefaultValue = 3)]
        public int BuyLimitOrderEntryOffsetPips { get; set; }

        [Parameter("# of Limit Orders", DefaultValue = 20)]
        public int NumberOfBuyLimitOrders { get; set; }

        [Parameter("Take profit for Limit Orders", DefaultValue = 0.5)]
        public double TPLimitOrders { get; set; }

        [Parameter("Buy Limit Order Min Volume (Lots)", DefaultValue = 5)]
        public int MinLimitOrderVolume { get; set; }

        [Parameter("Buy Limit Order Max Volume (Lots)", DefaultValue = 15)]
        public int MaxLimitOrderVolume { get; set; }

        [Parameter("# Limit Orders placed before Volume multiplies", DefaultValue = 2)]
        public int BuyLimitOrderVolumeMulitplierLevel { get; set; }

        [Parameter("Limit Order Volume multipler", DefaultValue = 1.5)]
        public double BuyLimitOrderVolumeMultipler { get; set; }

        [Parameter("# limit orders placed with Spacing level 1", DefaultValue = 5)]
        public int LimitOrderPlacedSpacingLevel1 { get; set; }

        [Parameter("Limit Order Spacing level 1 (Pips)", DefaultValue = 2)]
        public double LimitOrderSpacingLevel1 { get; set; }

        [Parameter("# limit orders placed with Spacing level 2", DefaultValue = 10)]
        public int LimitOrderPlacedSpacingLevel2 { get; set; }

        [Parameter("Limit Order Spacing level 2 (Pips)", DefaultValue = 1)]
        public double LimitOrderSpacingLevel2 { get; set; }

        [Parameter("Limit Order Spacing After Level 2 (Pips)", DefaultValue = 2)]
        public double LimitOrderSpacingLevel3 { get; set; }

        [Parameter("% of Limit Orders opened to indicate DOWN Spike", DefaultValue = 30)]
        public int BuyLimitOrdersOpenedSpikeIndicator { get; set; }

        // Buy positions config

        [Parameter("# of Buy positions placed", DefaultValue = 10)]
        public int NumberOfBuyPositions { get; set; }

        [Parameter("Buy Positions Min Volume (Lots)", DefaultValue = 20)]
        public int MinVolume { get; set; }

        [Parameter("Buy Positions Max Volume (Lots)", DefaultValue = 100)]
        public int MaxVolume { get; set; }

        [Parameter("# Buy positions placed before Volume and TP cycle", DefaultValue = 2)]
        public int BuyCycleFactor { get; set; }

        [Parameter("Buy Volume multiplier reducing from Max Volume", DefaultValue = 0.9)]
        public double BuyVolumeMultiplier { get; set; }

        [Parameter("Place buy positions interval (secs)", DefaultValue = 6)]
        public int BuyPositionsInterval { get; set; }

        [Parameter("# buy positions per interval", DefaultValue = 1)]
        public int NumberOfBuyPositionsPerInterval { get; set; }

        [Parameter("% of Buy positions closed to indicate UP Spike", DefaultValue = 40)]
        public int BuyPositionsOpenedSpikeIndicator { get; set; }

        //Take Profit config
        [Parameter("Buy Max Take Profit", DefaultValue = 0.5)]
        public double MaxTakeProfit { get; set; }

        [Parameter("Buy Min Take Profit", DefaultValue = 0.3)]
        public double MinTakeProfit { get; set; }

        [Parameter("Extra Take Profit Pips if Chase Level 2 reached", DefaultValue = 0.5)]
        public double ExtraChaseTargetPips { get; set; }

        [Parameter("Buy TP Increment", DefaultValue = 0.1)]
        public double TPIncrement { get; set; }

        //Chase Config

        [Parameter("Enable chase risk management", DefaultValue = true)]
        public bool chaseEnabled { get; set; }

        [Parameter("Retrace level Percentage", DefaultValue = 40)]
        public int retraceLevel { get; set; }

        [Parameter("Chase level 1 Percentage", DefaultValue = 20)]
        public int chaseLevel1 { get; set; }

        [Parameter("Chase level 2 Percentage", DefaultValue = 70)]
        public int chaseLevel2 { get; set; }

        //Stop Loss Config

        [Parameter("Last Order placed - Hard SL", DefaultValue = 5)]
        public double FinalOrderStopLoss { get; set; }

        [Parameter("Chase level 1 - Trailing fixed SL triggered", DefaultValue = 3)]
        public double TrailingStopPips { get; set; }

        [Parameter("Chase Level 2 - Hard SL triggered", DefaultValue = 20)]
        public double HardStopLoss { get; set; }

        protected MarketTimeInfo _marketTimeInfo;

        //Price and Position Variables
        protected double _spikeStartPrice;
        protected double _orderEntryStartPrice;
        protected string _lastPositionLabel;
        protected TradeType _lastPositionTradeType;
        protected double _lastPositionEntryPrice;
        protected double _lastClosedPositionEntryPrice;
        protected double _lastProfitPrice;

        //Count variables are doubles as required in % calculations
        protected double _openedPositionsCount = 0;
        protected double _closedPositionsCount = 0;
        protected double _openedSpikeUpPositionsCount = 0;
        protected double _openedSpikeDownBuyOrdersPositionsCount = 0;
        protected double _closedSpikeUpPositionsCount = 0;
        protected double _closedSpikeDownBuyOrdersPositionsCount = 0;
        protected double _orderCountLabel = 0;

        //Stop Loss Variables
        protected bool _IsSpikeDirectionDown = false;
        protected double _divideTrailingStopPips = 1;
        protected bool _isManagePositionRiskActive = false;
        protected bool _isTrailingStopsActive = false;
        protected bool _isHardSLLastProfitPrice = false;
        protected bool _isHardSLLastPositionEntryPrice = false;
        protected bool _isHardSLActive = false;
        protected bool _isHardSLSet = false;
        protected bool _isTriggeredChaseLevel1 = false;
        protected bool _isTriggeredChaseLevel2 = false;
        protected bool _isSetBreakEvenSLAtMinProfitActive = false;
        protected bool _isSetHardBreakEvenSLActive = false;
        protected bool _IsNewTPChaseActive = false;
        protected bool _isNewTPChaseSet = false;
        protected bool _IsSpikeStartBreakEvenSet = false;

        //State Variables
        protected bool _isPendingOrdersClosed = false;
        protected bool _startPriceCaptured = false;
        protected bool _earlyEntryPriceCaptured = false;
        protected bool _ordersPlaced = false;
        protected bool _ordersRequested = false;
        protected bool _positionsPlaced = false;
        protected bool _positionsRequested = false;
        protected bool _isTerminated = false;
        protected bool _isReset = true;
        protected bool _isCloseTime = false;
        protected bool _isReducedRiskTime = false;
        protected int _orderCount = 0;
        protected int _totalIntervalSecs = 0;

        protected string _botId = null;

        List<string> debugCSV = new List<string>();

        //Performance Reporting
        protected double _dayProfitTotal = 0;
        protected double _dayPipsTotal = 0;
        protected double _spikePeakPips = 0;
        protected double _spikeUpPeakPips = 0;
        protected double _spikeDownPeakPips = 0;
        protected double _spikeUpPeakPrice = 0;
        protected double _spikeDownPeakPrice = 0;

        protected override void OnStart()
        {
            _botId = generateBotId();
            _marketTimeInfo = new MarketTimeInfo();
            setTimeZone();
            Timer.Start(1);
            //start timer with 1 second interval
            Positions.Opened += OnPositionsOpened;
            Positions.Closed += OnPositionsClosed;
            debugCSV.Add("PARAMETERS");
            debugCSV.Add("NumberOfOrders," + NumberOfBuyPositions.ToString());
            debugCSV.Add("Volume," + MinVolume.ToString());
            debugCSV.Add("VolumeMax," + MaxVolume.ToString());
            debugCSV.Add("OrderVolumeLevels," + BuyLimitOrderVolumeMulitplierLevel.ToString());
            debugCSV.Add("VolumeMultipler," + BuyVolumeMultiplier.ToString());
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

        protected override void OnTimer()
        {

            if (IsTradingTimeIn(1))
            {
                if (!_earlyEntryPriceCaptured)
                {
                    //Get the Price 1min before open
                    _orderEntryStartPrice = Symbol.Bid;
                    _earlyEntryPriceCaptured = true;
                }

                if (!_ordersPlaced && !_ordersRequested)
                {
                    placeBuyLimitOrders();
                }
            }

            // If backtesting use the Server.Time.        
            if (IsOpenTradingTime())
            {
                //Start Trading
                if (_isReset)
                    _isReset = false;

                if (!_startPriceCaptured)
                {
                    //Get the Market Open Price
                    _spikeStartPrice = Symbol.Bid;
                    _startPriceCaptured = true;
                }

                if (IsTradingTimeAfterSeconds(_totalIntervalSecs))
                {
                    if (!_positionsPlaced && !_positionsRequested)
                    {
                        placeBuyOrders();
                    }
                    _totalIntervalSecs += BuyPositionsInterval;
                }

                captureSpikePeak();
            }

            //It is outside Placing Trading Time
            if (IsCloseTradingTime())
            {
                _isCloseTime = true;

                //CalcSpikeDirection
                _IsSpikeDirectionDown = isSpikeDown(false);

                //Cancel all open pending Orders
                CancelAllPendingOrders();

                if (_ordersPlaced || _positionsPlaced)
                {
                    if (_openedPositionsCount - _closedPositionsCount > 0)
                    {
                        //If a spike has not happened then set breakeven SL
                        if (_spikePeakPips < SpikeIndicatorPips)
                        {
                            _isSetHardBreakEvenSLActive = true;
                        }

                        //If Spike down and min Buy profit has not been reached
                        if (_IsSpikeDirectionDown && _closedSpikeUpPositionsCount < 1)
                        {
                            _isSetHardBreakEvenSLActive = true;
                        }

                        //Positions still open after ReducePositionRiskTime
                        if (!_isReducedRiskTime && _marketTimeInfo.IsReduceRiskTime(IsBacktesting, Server.Time, ReducePositionRiskTime))
                        {
                            _isReducedRiskTime = true;
                        }

                        //If trades still open at ClosingAllTime then take the hit and close remaining positions
                        if (!_isTerminated && _marketTimeInfo.IsTerminateTime(IsBacktesting, Server.Time))
                        {
                            CloseAllPositions();
                            _isTerminated = true;
                        }
                    }

                    //Set Hard Stop Loss
                    _isHardSLActive = true;
                    _isSetBreakEvenSLAtMinProfitActive = true;

                    //Activate manage position risk if it is not already active
                    if (!_isManagePositionRiskActive)
                        _isManagePositionRiskActive = true;

                    //Out of Trading time and all positions that were opened are now closed
                    if (_openedPositionsCount > 0 && _openedPositionsCount - _closedPositionsCount == 0)
                        ResetSwordFish();
                }
            }

            if (_isManagePositionRiskActive)
            {
                //Manage the open positions
                ManagePositionRisk();
            }
        }

        protected void ManagePositionRisk()
        {
            //Activated on either Buy Positions SPIKE UP Indicator triggered OR Into Close Time.

            if (chaseEnabled)
            {
                //Calculate spike retrace factor
                double chaseFactor = calculateChaseFactorForSpikeUp();

                //ChaseLevel1 has been passed
                if (chaseLevel1 <= chaseFactor)
                {
                    //Set Hard SL
                    _isHardSLActive = true;

                    //Activate Trailing Stop Losses
                    _isSetBreakEvenSLAtMinProfitActive = true;

                    _isTriggeredChaseLevel1 = true;
                }

                //ChaseLevel2 has been passed
                if (chaseLevel2 <= chaseFactor)
                {
                    _IsNewTPChaseActive = true;
                    _isTriggeredChaseLevel2 = true;
                }

                //Modify Orders based on Position Risk indicators
                ManagePositions();
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
            if (_spikeStartPrice < Symbol.Ask)
            {
                //OptionsBot only has Buy positions - look for highest Ask price (to close Buy position) above Spike Start
                if (Symbol.Ask > _spikeUpPeakPrice || _spikeUpPeakPrice == 0)
                {
                    _spikeUpPeakPrice = Symbol.Ask;
                    _spikeUpPeakPips = Symbol.Ask - _spikeStartPrice;
                    updateSpikePeakPips(_spikeUpPeakPips);
                }
            }
            else if (_spikeStartPrice > Symbol.Bid)
            {
                //Look for lowest Bid (to enter Buy position) below spike Start
                if (Symbol.Bid < _spikeDownPeakPrice || _spikeDownPeakPrice == 0)
                {
                    _spikeDownPeakPrice = Symbol.Bid;
                    _spikeDownPeakPips = _spikeStartPrice - Symbol.Bid;
                    updateSpikePeakPips(_spikeDownPeakPips);
                }
            }
        }

        protected void updateSpikePeakPips(double newSpikePeakPips)
        {
            if (_spikePeakPips < newSpikePeakPips)
                _spikePeakPips = newSpikePeakPips;
        }

        protected void ManagePositions()
        {
            foreach (Position p in Positions)
            {
                {
                    try
                    {
                        if (isThisBotId(p.Label) && IsSpikeUpBuyPosition(p.Label))
                        {
                            {
                                ModifyPositionAsync(p, calcNewStopLoss(p), calcNewTakeProfit(p), OnModifyTrailingStop);
                            }
                        }
                    } catch (Exception Ex)
                    {
                        Print("FAILED to manage position: " + Ex.ToString());
                    }
                }
            }
        }

        protected double calcNewStopLoss(Position p)
        {
            //This needs to manage positions TP (negative TP) instead of using SL and use the overall profit from all positions to calculate BreakEven.

            double currentSLPrice = 0;
            double trailingSLPrice = 0;
            double hardSLPrice = 0;
            double breakEvenSLPrice = 0;
            List<double> SLPrices = new List<double>();

            //If this is a big Spike down wait for Chase Level 1 retrace before setting SL's
            if (_IsSpikeDirectionDown)
            {
                if (_closedSpikeDownBuyOrdersPositionsCount < _openedSpikeDownBuyOrdersPositionsCount * retraceLevel / 100)
                {
                    return 0;
                }
            }

            if (p.StopLoss.HasValue)
            {
                currentSLPrice = (double)p.StopLoss;
                SLPrices.Add(currentSLPrice);
            }

            if (!_isHardSLSet && _isHardSLActive)
            {
                hardSLPrice = calcHardSLPrice(p);
                SLPrices.Add(hardSLPrice);
                _isHardSLSet = true;
            }

            if (_isSetBreakEvenSLAtMinProfitActive || _isSetHardBreakEvenSLActive)
            {
                breakEvenSLPrice = calcBreakEvenSLPrice(p);
                SLPrices.Add(breakEvenSLPrice);
            }

            if (_isTrailingStopsActive)
            {
                trailingSLPrice = calcTrailingSLPrice(p);
                SLPrices.Add(trailingSLPrice);
            }

            return calcClosestSL(p.TradeType, SLPrices);
        }

        protected double calcNewTakeProfit(Position p)
        {
            double currentTPPrice = 0;
            double newChaseTP = 0;

            if (p.TakeProfit.HasValue)
                currentTPPrice = (double)p.TakeProfit;

            if (!_isNewTPChaseSet && _IsNewTPChaseActive)
            {
                newChaseTP = calcNewChaseTPPrice(p);
                _isNewTPChaseSet = true;
            }

            List<double> TPPrices = new List<double> 
            {
                currentTPPrice,
                newChaseTP
            };

            return calcFurthestTP(p.TradeType, TPPrices);
        }


        protected double calcNewChaseTPPrice(Position p)
        {
            double newChaseTPPrice = 0;
            if (p.TradeType == TradeType.Buy)
            {
                newChaseTPPrice = (double)p.TakeProfit + ExtraChaseTargetPips * (1 / Symbol.PipSize);
            }

            if (p.TradeType == TradeType.Sell)
            {
                newChaseTPPrice = (double)p.TakeProfit - ExtraChaseTargetPips * (1 / Symbol.PipSize);
                ;
            }
            return newChaseTPPrice;
        }

        protected double calcHardSLPrice(Position p)
        {
            double hardPrice = p.EntryPrice;
            if (_lastPositionEntryPrice > 0)
                hardPrice = _lastPositionEntryPrice;

            double hardSLPrice = 0;
            if (p.TradeType == TradeType.Buy)
            {
                hardSLPrice = hardPrice - HardStopLoss;
            }

            if (p.TradeType == TradeType.Sell)
            {
                hardSLPrice = hardPrice - HardStopLoss;
            }
            return isValidSL(p.TradeType, hardSLPrice);
        }


        protected double isValidSL(TradeType tradeType, double newSLPrice)
        {
            double validSLPrice = newSLPrice;
            if (tradeType == TradeType.Buy)
            {
                //Is newStopLoss more than the current Ask and therefore not valid
                if (newSLPrice > Symbol.Ask)
                    validSLPrice = 0;
            }

            if (tradeType == TradeType.Sell)
            {
                //Is newStopLoss more than the current Ask and therefore not valid
                if (newSLPrice < Symbol.Bid)
                    validSLPrice = 0;
            }
            return validSLPrice;
        }


        protected void setBreakEvenTPForBuySpikePositions()
        {
            foreach (Position p in Positions)
            {
                try
                {
                    if (isThisBotId(p.Label))
                    {
                        //Set the newTP to Breakeven or Spike Start
                        double newTP = _spikeStartPrice;
                        if (IsSpikeDownBuyLimitOrder(p.Label))
                        {
                            //check if TP for Buy Limit Order is already closer than Spike Start
                            if (isTPCloserThanSpikeStart(p))
                                continue;
                        }
                        else
                        {
                            //If Buy position set TP to BreakEven
                            newTP = p.EntryPrice;
                        }

                        if (_lastPositionTradeType == TradeType.Buy)
                        {
                            ModifyPositionAsync(p, 0, newTP, OnModifySpikeStartStopComplete);
                        }

                        if (_lastPositionTradeType == TradeType.Sell)
                        {
                            ModifyPositionAsync(p, 0, newTP, OnModifySpikeStartStopComplete);
                        }
                    }
                } catch (Exception e)
                {
                    Print("Failed to Modify Position:" + e.Message);
                }
            }
        }

        protected bool IsSpikeDownBuyLimitOrder(string positionLabel)
        {
            string id = positionLabel.Substring(7, 7);
            if (id.Equals("O"))
                return true;
            else
                return false;
        }

        protected bool IsSpikeUpBuyPosition(string positionLabel)
        {
            string id = positionLabel.Substring(7, 7);
            if (id.Equals("X"))
                return true;
            else
                return false;
        }

        protected double calcBreakEvenSLPrice(Position p)
        {
            double breakEvenSL = 0;
            double minBreakEvenProfit = MinTakeProfit;

            if (_isSetHardBreakEvenSLActive)
            {
                minBreakEvenProfit = 0;
            }

            if (_lastPositionTradeType == TradeType.Buy)
            {
                if (Symbol.Ask - minBreakEvenProfit * (1 / Symbol.PipSize) > p.EntryPrice)
                {
                    breakEvenSL = p.EntryPrice;
                }
            }

            if (_lastPositionTradeType == TradeType.Sell)
            {
                if (Symbol.Bid + minBreakEvenProfit * (1 / Symbol.PipSize) < p.EntryPrice)
                {
                    breakEvenSL = p.EntryPrice;
                }
            }
            return breakEvenSL;
        }

        protected bool isTPCloserThanSpikeStart(Position p)
        {
            if (p.TakeProfit.HasValue)
            {
                switch (p.TradeType)
                {
                    case TradeType.Sell:
                        {
                            return p.TakeProfit.Value >= _spikeStartPrice;
                        }
                    case TradeType.Buy:
                        {
                            return p.TakeProfit.Value <= _spikeStartPrice;
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

        protected double calcFurthestTP(TradeType tradetype, List<double> TPPrices)
        {
            double newTakeProfitPrice = 0;
            switch (tradetype)
            {
                case TradeType.Sell:
                    {
                        //Sort the list to get the Lowest price first
                        List<double> TPSortedPrices = TPPrices.OrderBy(d => d).ToList();
                        newTakeProfitPrice = TPSortedPrices[0];
                    }
                    break;
                case TradeType.Buy:
                    {
                        //Sort the list to get the highest price first
                        List<double> TPSortedPrices = TPPrices.OrderByDescending(d => d).ToList();
                        newTakeProfitPrice = TPSortedPrices[0];
                    }
                    break;
            }
            return newTakeProfitPrice;
        }

        protected double calcClosestSL(TradeType tradetype, List<double> SLPrices)
        {

            double newStopLossPrice = 0;
            switch (tradetype)
            {
                case TradeType.Sell:
                    {
                        //Sort the list to get the Lowest price first
                        List<double> SLSortedPrices = SLPrices.OrderBy(d => d).ToList();
                        newStopLossPrice = SLSortedPrices[0];
                    }
                    break;
                case TradeType.Buy:
                    {
                        //Sort the list to get the highest price first
                        List<double> SLSortedPrices = SLPrices.OrderByDescending(d => d).ToList();
                        newStopLossPrice = SLSortedPrices[0];
                    }
                    break;
            }
            return newStopLossPrice;
        }


        //calculate Trailing Stop Loss
        protected double calcTrailingSLPrice(Position position)
        {
            double newTrailingSLPrice = 0;

            if (position.TradeType == TradeType.Buy)
            {
                newTrailingSLPrice = Symbol.Ask - TrailingStopPips / _divideTrailingStopPips;
                newTrailingSLPrice = isValidSL(position.TradeType, newTrailingSLPrice);
            }

            if (position.TradeType == TradeType.Sell)
            {
                newTrailingSLPrice = Symbol.Bid + TrailingStopPips / _divideTrailingStopPips;
                newTrailingSLPrice = isValidSL(position.TradeType, newTrailingSLPrice);
            }
            return newTrailingSLPrice;
        }

        // Place Buy Orders
        protected void placeBuyOrders()
        {
            //Place Buy Limit Orders
            for (int n = 0; n < NumberOfBuyPositionsPerInterval; n++)
            {
                try
                {
                    tradeData data = new tradeData 
                    {
                        tradeType = TradeType.Buy,
                        symbol = Symbol,
                        volume = setBuyVolume(_orderCount),
                        entryPrice = 0,
                        label = _botId + "-X-SWF#" + _orderCountLabel + "-" + _marketTimeInfo.market + "-" + "-" + getTimeStamp(),
                        stopLossPips = 0,
                        takeProfitPips = calcBuyTakeProfit(_orderCount)
                    };
                    if (data == null)
                        continue;

                    //Place Market Orders immediately
                    ExecuteMarketOrderAsync(data.tradeType, data.symbol, data.volume, data.label, data.stopLossPips, data.takeProfitPips, OnPlaceTradeOperationComplete);
                    _orderCountLabel++;
                    _orderCount++;
                } catch (Exception e)
                {
                    Print("Failed to place Buy Limit Order: " + e.Message);
                }

                //If all orders have been sent set flag
                if (_orderCountLabel > NumberOfBuyPositions - 1)
                    _positionsRequested = true;
            }
        }

        // Place Buy Limit Orders
        protected void placeBuyLimitOrders()
        {
            //Place Buy Limit Orders
            for (int OrderCount = 0; OrderCount < NumberOfBuyLimitOrders; OrderCount++)
            {
                try
                {
                    tradeData data = new tradeData 
                    {
                        tradeType = TradeType.Buy,
                        symbol = Symbol,
                        volume = setBuyOrderLimitVolume(OrderCount),
                        entryPrice = calcBuyOrderEntryPrice(OrderCount),
                        label = _botId + "-O-SWF#" + OrderCount + "-" + _marketTimeInfo.market + "-" + getTimeStamp(),
                        stopLossPips = 0,
                        takeProfitPips = calcBuyLimitTakeProfit(OrderCount)
                    };
                    if (data == null)
                        continue;

                    //Check that entry price is valid
                    if (data.entryPrice < Symbol.Bid)
                    {
                        PlaceLimitOrderAsync(data.tradeType, data.symbol, data.volume, data.entryPrice, data.label, data.stopLossPips, data.takeProfitPips, OnPlaceOrderOperationComplete);
                    }
                    else
                    {
                        //Tick price has 'jumped' - therefore avoid placing all PendingOrders by re-calculating the OrderCount to the equivelant entry point.
                        OrderCount = calculateNewOrderCount(NumberOfBuyLimitOrders, OrderCount, Symbol.Ask);
                        ExecuteMarketOrderAsync(data.tradeType, data.symbol, data.volume, data.label + "X", data.stopLossPips, data.takeProfitPips, OnPlaceOrderOperationComplete);
                    }
                } catch (Exception e)
                {
                    Print("Failed to place Buy Limit Order: " + e.Message);
                }
                if (OrderCount > 0)
                    _ordersRequested = true;
            }
        }

        protected double calcBuyOrderEntryPrice(int orderCount)
        {
            double offset = BuyLimitOrderEntryOffsetPips;

            //Ordercount less than Level1 spacing level
            if (orderCount < LimitOrderPlacedSpacingLevel1)
            {
                offset += orderCount * LimitOrderSpacingLevel1;
            }

            //Ordercount between Level1 and level2 spacing level
            if (orderCount >= LimitOrderPlacedSpacingLevel1 && orderCount < LimitOrderPlacedSpacingLevel1 + LimitOrderPlacedSpacingLevel2)
            {
                offset += LimitOrderPlacedSpacingLevel1 * LimitOrderSpacingLevel1;
                offset += (orderCount - LimitOrderPlacedSpacingLevel1) * LimitOrderSpacingLevel2;
            }

            //Ordercount greater than # orders placed at Level 1 and Level2 spacing
            if (orderCount >= LimitOrderPlacedSpacingLevel1 + LimitOrderPlacedSpacingLevel2)
            {
                offset += LimitOrderPlacedSpacingLevel1 * LimitOrderSpacingLevel1;
                offset += LimitOrderPlacedSpacingLevel2 * LimitOrderSpacingLevel2;
                offset += (orderCount - LimitOrderPlacedSpacingLevel1 - LimitOrderPlacedSpacingLevel2) * LimitOrderSpacingLevel3;
            }

            return _orderEntryStartPrice - offset;
        }

        //Set a stop loss on the last Pending Order set to catch the break away train that never comes back!
        protected double setPendingOrderStopLossPips(int orderCount, int numberOfOrders)
        {
            if (orderCount == numberOfOrders - 1)
            {
                return FinalOrderStopLoss * (1 / Symbol.PipSize);
            }
            else
            {
                return 0;
            }
        }

        //Calculate a new orderCount number for when tick jumps
        protected int calculateNewOrderCount(int numberOfOrders, int orderCount, double currentTickPrice)
        {
            double tickJumpIntoRange = Math.Abs(_spikeStartPrice - currentTickPrice);
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

        protected void OnModifySpikeStartStopComplete(TradeResult tr)
        {
            OnTradeOperationComplete(tr, "FAILED to modify SPIKE START stop loss: ");
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

        protected void OnModifyNewChaseTPTarget(TradeResult tr)
        {
            OnTradeOperationComplete(tr, "FAILED to modify TAKE PROFIT: ");
            if (tr.IsSuccessful)
                _IsNewTPChaseActive = true;
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
            if (tr.IsSuccessful && _orderCountLabel > NumberOfBuyPositions - 1)
                _positionsPlaced = true;
            else
                OnTradeOperationComplete(tr, "FAILED to enter TRADE position: ");
        }

        protected void OnTradeOperationComplete(TradeResult tr, string errorMsg)
        {
            //if (!tr.IsSuccessful)
            //{
            //    if (tr.Position != null)
            //        Print(errorMsg + tr.Error, " Position: ", tr.Position.Label, " ", tr.Position.TradeType, " ", Time);
            //    if (tr.PendingOrder != null)
            //        Print(errorMsg + tr.Error, " PendingOrder: ", tr.PendingOrder.Label, " ", tr.PendingOrder.TradeType, " ", Time);
            //}
        }

        protected double calcBuyLimitTakeProfit(int orderCount)
        {
            return TPLimitOrders * (1 / Symbol.PipSize);

        }

        protected double calcBuyTakeProfit(int orderCount)
        {
            double tp = 0;
            double TPCycleNumber = orderCount % BuyCycleFactor;

            //Use Min TP for the first order and biggest volume order in the cycle
            tp = MinTakeProfit + (TPCycleNumber) * TPIncrement;
            tp *= (1 / Symbol.PipSize);

            if (tp > MaxTakeProfit * (1 / Symbol.PipSize))
                tp = MaxTakeProfit * (1 / Symbol.PipSize);

            return Math.Round(tp, 1);
        }

        //protected double calcAscendingTakeProfit(int orderCount)
        //{
        //    double tp = 0;

        //    tp = MaxTakeProfit - (1 / Symbol.PipSize) - (orderCount - 1) * TPSpacingBase;

        //    if (tp < MinTakeProfit * (1 / Symbol.PipSize))
        //        tp = MinTakeProfit * (1 / Symbol.PipSize);

        //    return tp;
        //}

        //protected void setCascadingTakeProfit()
        //{
        //    IEnumerable<Position> orderedPositions = Positions.OrderBy(position => position.EntryPrice);

        //    int positionCount = 0;
        //    foreach (Position p in orderedPositions)
        //    {
        //        try
        //        {
        //            if (isThisBotId(p.Label))
        //            {
        //                ModifyPositionAsync(p, p.StopLoss, p.EntryPrice + calcAscendingTakeProfit(positionCount) * Symbol.PipSize, OnModifyTakeProfitComplete);
        //            }
        //        } catch (Exception e)
        //        {
        //            Print("Failed to Modify Position: " + e.Message);
        //        }
        //        positionCount++;
        //    }
        //}

        protected void OnPositionsOpened(PositionOpenedEventArgs args)
        {
            if (isThisBotId(args.Position.Label))
            {
                updateOpenPositionCounts(args.Position.Label);

                //Capture the last position info for the first Buy Position if no Orders have been opened
                if (IsSpikeDownBuyLimitOrder(args.Position.Label) && _openedSpikeDownBuyOrdersPositionsCount < 1)
                {
                    _lastPositionTradeType = args.Position.TradeType;
                    _lastPositionEntryPrice = args.Position.EntryPrice;
                    _lastPositionLabel = args.Position.Label;
                }


                //Only Capture last Position info for Pending Order entries i.e. the furthest away
                if (IsSpikeDownBuyLimitOrder(args.Position.Label))
                {
                    _lastPositionTradeType = args.Position.TradeType;
                    _lastPositionEntryPrice = args.Position.EntryPrice;
                    _lastPositionLabel = args.Position.Label;
                }

                //If Spike Down Buy Orders have opened but are fewer than SPIKE DOWN indicator % of orders then this is still a spike UP and reset the TP
                if (0 < _openedSpikeDownBuyOrdersPositionsCount && _openedSpikeDownBuyOrdersPositionsCount < NumberOfBuyLimitOrders * BuyLimitOrdersOpenedSpikeIndicator / 100)
                    //setCascadingTakeProfit(); Doesn't make much sense to do this given buy positions have scaled TP's and Limit orders have fixed TP's

                    //If the number of Buy Order positions opened is greater than SPIKE DOWN indicator % of orders then spike is DOWN
                    if (isSpikeDown(false))
                    {
                        setBreakEvenTPForBuySpikePositions();
                    }

            }
        }


        protected bool isSpikeDown(bool positionClosed)
        {
            if (_openedSpikeDownBuyOrdersPositionsCount > NumberOfBuyLimitOrders * BuyLimitOrdersOpenedSpikeIndicator / 100)
            {
                return true;
            }

            if (_isCloseTime && _spikeDownPeakPips > _spikeUpPeakPips)
            {
                return true;
            }

            if (positionClosed && _spikeDownPeakPips > _spikeUpPeakPips)
            {
                return true;
            }
            return false;
        }



        protected void OnPositionsClosed(PositionClosedEventArgs args)
        {
            if (isThisBotId(args.Position.Label))
            {
                updateClosedPositionCounts(args.Position.Label);
                updateProfitReporting(args.Position);

                //Taking profit
                if (args.Position.GrossProfit >= 0)
                {
                    //capture last position take profit price
                    setLastProfitPrice(args.Position.TradeType);

                    //capture last closed position entry price
                    _lastClosedPositionEntryPrice = args.Position.EntryPrice;

                    //Set trailing SL
                    _isTrailingStopsActive = true;

                    //If Buy Positions SPIKE UP Indicator triggered
                    if (_closedSpikeUpPositionsCount > NumberOfBuyPositions * BuyPositionsOpenedSpikeIndicator / 100)
                    {
                        CancelAllPendingOrders();

                        //activate manage positions
                        _isManagePositionRiskActive = true;
                    }

                    //If Spike Down then set Buy Positions to breakEven
                    if (isSpikeDown(true))
                    {
                        //activate manage positions
                        _isSetHardBreakEvenSLActive = true;
                        ManagePositions();
                    }
                }
            }
        }

        protected void updateProfitReporting(Position p)
        {
            _dayProfitTotal += p.GrossProfit;
            _dayPipsTotal += p.Pips;
            debugCSV.Add(p.Label + "," + p.GrossProfit + "," + p.Pips + "," + p.EntryPrice + "," + History.FindLast(p.Label, Symbol, p.TradeType).ClosingPrice + "," + p.StopLoss + "," + p.TakeProfit + "," + Time.DayOfWeek + "," + Time + debugState());
        }


        protected void updateClosedPositionCounts(string positionLabel)
        {
            _closedPositionsCount++;

            if (IsSpikeUpBuyPosition(positionLabel))
            {
                _closedSpikeUpPositionsCount++;
            }

            if (IsSpikeDownBuyLimitOrder(positionLabel))
            {
                _closedSpikeDownBuyOrdersPositionsCount++;
            }
        }

        protected void updateOpenPositionCounts(string positionLabel)
        {
            _openedPositionsCount++;

            if (IsSpikeUpBuyPosition(positionLabel))
            {
                _openedSpikeUpPositionsCount++;
            }

            if (IsSpikeDownBuyLimitOrder(positionLabel))
            {
                _openedSpikeDownBuyOrdersPositionsCount++;
            }
        }

        protected double calculateChaseFactorForSpikeUp()
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

        protected bool IsOpenTradingTime()
        {
            return _marketTimeInfo.IsPlacePendingOrdersTime(IsBacktesting, Server.Time);
        }

        protected bool IsCloseTradingTime()
        {
            return _marketTimeInfo.IsCloseTime(IsBacktesting, Server.Time);
        }

        protected bool IsTradingTimeIn(int mins)
        {
            return _marketTimeInfo.IsTimeBeforeOpen(IsBacktesting, Server.Time, mins);
        }

        protected bool IsTradingTimeAfter(int mins)
        {
            return _marketTimeInfo.IsTimeAfterMinsFromOpen(IsBacktesting, Server.Time, mins);
        }

        protected bool IsTradingTimeAfterSeconds(int secs)
        {
            return _marketTimeInfo.IsTimeAfterSecsFromOpen(IsBacktesting, Server.Time, secs);
        }

        //Increase the volume based on Orders places and volume levels and multiplier until max volume reached
        protected int setBuyVolume(int orderCount)
        {

            double BuyVolumeCycleNumber = orderCount % BuyCycleFactor;

            //Place the largest orders first using MaxVolume
            double volume = Math.Floor(MaxVolume * Math.Pow(BuyVolumeMultiplier, BuyVolumeCycleNumber));

            if (volume < MinVolume)
            {
                volume = MinVolume;
            }

            return (int)volume;
        }

        //Increase the volume based on Orders places and volume levels and multiplier until max volume reached
        protected int setBuyOrderLimitVolume(int orderCount)
        {

            double BuyLimitOrderVolumeLevel = orderCount / BuyLimitOrderVolumeMulitplierLevel;
            double volume = Math.Pow(BuyLimitOrderVolumeMultipler, BuyLimitOrderVolumeLevel) * MinLimitOrderVolume;

            if (volume > MaxLimitOrderVolume)
            {
                volume = MaxLimitOrderVolume;
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
            _openedSpikeUpPositionsCount = 0;
            _openedSpikeDownBuyOrdersPositionsCount = 0;
            _closedSpikeUpPositionsCount = 0;
            _closedSpikeDownBuyOrdersPositionsCount = 0;
            _orderCountLabel = 0;

            //reset Last Position variables
            _lastPositionLabel = "NO LAST POSITION SET";
            _lastPositionEntryPrice = 0;
            _lastClosedPositionEntryPrice = 0;
            _lastProfitPrice = 0;

            //reset risk management variables
            _isManagePositionRiskActive = false;
            _divideTrailingStopPips = 1;
            _isTrailingStopsActive = false;
            _isSetBreakEvenSLAtMinProfitActive = false;
            _IsNewTPChaseActive = false;
            _isNewTPChaseSet = false;
            _isHardSLActive = false;
            _isHardSLSet = false;
            _isHardSLLastPositionEntryPrice = false;
            _isHardSLLastProfitPrice = false;
            _IsSpikeStartBreakEvenSet = false;

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
            _spikeUpPeakPrice = 0;
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
            state += "," + _isSetBreakEvenSLAtMinProfitActive;
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
                    // Market for Options trades opens at 10:09:50 am.
                    _marketTimeInfo.open = new TimeSpan(10, 9, 0);
                    // Market for swordfish trades closes at 10:13am.
                    _marketTimeInfo.close = _marketTimeInfo.open.Add(TimeSpan.FromMinutes(CloseTime));
                    // Close all open Swordfish position at 11:29am before US opens.
                    _marketTimeInfo.closeAll = new TimeSpan(11, 29, 0);

                    break;
                case "GER30":
                    _marketTimeInfo.market = "DAX";
                    _marketTimeInfo.tz = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                    // Market for swordfish opens at 9:00.
                    _marketTimeInfo.open = new TimeSpan(9, 0, 0);
                    // Market for swordfish closes at 9:05.
                    _marketTimeInfo.close = new TimeSpan(9, 5, 0);
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

    //Is the current time within the trading period orders/positions can be placed
    public bool IsPlacePendingOrdersTime(bool isBackTesting, DateTime serverTime)
    {
        if (isBackTesting)
        {
            return IsBetweenOpenAndCloseTime(serverTime);
        }
        else
        {
            return IsBetweenOpenAndCloseTime(DateTime.UtcNow);
        }
    }

    //Is after Close time
    public bool IsCloseTime(bool isBackTesting, DateTime serverTime)
    {

        if (isBackTesting)
        {
            return IsAfterCloseTime(serverTime);
        }
        else
        {
            return IsAfterCloseTime(DateTime.UtcNow);
        }
    }

    //Is after Reduce Risk time
    public bool IsReduceRiskTime(bool isBackTesting, DateTime serverTime, int reduceRiskTimeFromOpen)
    {
        if (isBackTesting)
        {
            return IsTimeAfterMinsFromOpen(serverTime, reduceRiskTimeFromOpen);
        }
        else
        {
            return IsTimeAfterMinsFromOpen(DateTime.UtcNow, reduceRiskTimeFromOpen);
        }
    }

    //Time X minutes from open
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

    //Time X minutes from open
    public bool IsTimeAfterMinsFromOpen(bool isBackTesting, DateTime serverTime, int timeAfterOpen)
    {
        if (isBackTesting)
        {
            return IsTimeAfterMinsFromOpen(serverTime, timeAfterOpen);
        }
        else
        {
            return IsTimeAfterMinsFromOpen(DateTime.UtcNow, timeAfterOpen);
        }
    }

    //Time X minutes from open
    public bool IsTimeAfterSecsFromOpen(bool isBackTesting, DateTime serverTime, int secsAfterOpen)
    {
        if (isBackTesting)
        {
            return IsTimeAfterSecsFromOpen(serverTime, secsAfterOpen);
        }
        else
        {
            return IsTimeAfterSecsFromOpen(DateTime.UtcNow, secsAfterOpen);
        }
    }

    //Is after Terminate time
    public bool IsTerminateTime(bool isBackTesting, DateTime serverTime)
    {

        if (isBackTesting)
        {
            return IsAfterTerminateTime(serverTime);
        }
        else
        {
            return IsAfterTerminateTime(DateTime.UtcNow);
        }
    }

    //Is the current time within the period Swordfish Pending Orders can be placed.
    private bool IsBetweenOpenAndCloseTime(DateTime dateTimeUtc)
    {
        DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, tz);
        return (tzTime.TimeOfDay >= open & tzTime.TimeOfDay <= close);
    }

    //Is the current time after the trading open time in mins
    private bool IsTimeAfterMinsFromOpen(DateTime dateTimeUtc, int minsFromOpen)
    {
        DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, tz);
        return (tzTime.TimeOfDay >= open.Add(TimeSpan.FromMinutes(minsFromOpen)));
    }

    //Is the current time after the trading open time in secs
    private bool IsTimeAfterSecsFromOpen(DateTime dateTimeUtc, int secsFromOpen)
    {
        DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, tz);
        return (tzTime.TimeOfDay >= open.Add(TimeSpan.FromSeconds(secsFromOpen)));
    }

    //Is the current time after the time period when risk should be reduced.
    private bool IsTimeBeforeOpen(DateTime dateTimeUtc, int timeToOpen)
    {
        DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, tz);
        return (tzTime.TimeOfDay < open && tzTime.TimeOfDay >= open.Subtract(TimeSpan.FromMinutes(timeToOpen)));
    }

    //Is the current time within the period Swordfish positions can remain open.
    private bool IsAfterTerminateTime(DateTime dateTimeUtc)
    {
        DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, tz);
        return tzTime.TimeOfDay >= closeAll;
    }

    //Is the current time within the period Swordfish positions can remain open.
    private bool IsAfterCloseTime(DateTime dateTimeUtc)
    {
        DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, tz);
        return tzTime.TimeOfDay >= close;
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









