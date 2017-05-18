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

        [Parameter("# of Orders", DefaultValue = 8)]
        public int NumberOfOrders { get; set; }

        [Parameter("Volume (Lots)", DefaultValue = 20)]
        public int Volume { get; set; }

        [Parameter("Volume Max (Lots)", DefaultValue = 100)]
        public int VolumeMax { get; set; }

        [Parameter("# Order placed before Volume multiplies", DefaultValue = 2)]
        public int OrderVolumeLevels { get; set; }

        [Parameter("Volume multipler", DefaultValue = 2)]
        public double VolumeMultipler { get; set; }

        [Parameter("Take Profit", DefaultValue = 1.0)]
        public double TakeProfit { get; set; }

        [Parameter("Minimum Take Profit", DefaultValue = 0.2)]
        public double MinTakeProfit { get; set; }

        [Parameter("Mins after swordfish period to reduce position risk", DefaultValue = 45)]
        public int ReducePositionRiskTime { get; set; }

        [Parameter("Enable chase risk management", DefaultValue = true)]
        public bool chaseEnabled { get; set; }

        [Parameter("Chase level 1 Percentage", DefaultValue = 33)]
        public int chaseLevel1 { get; set; }

        [Parameter("Chase level 2 Percentage", DefaultValue = 50)]
        public int chaseLevel2 { get; set; }

        [Parameter("Chase level 3 Percentage", DefaultValue = 66)]
        public int chaseLevel3 { get; set; }

        [Parameter("Initial Hard SL for last Order placed", DefaultValue = 5)]
        public double FinalOrderStopLoss { get; set; }

        [Parameter("Triggered Hard SL buffer", DefaultValue = 20)]
        public double HardStopLossBuffer { get; set; }

        [Parameter("Trailing SL fixed distance", DefaultValue = 5)]
        public double TrailingStopPips { get; set; }

        protected MarketTimeInfo _marketTimeInfo;

        //Price and Position Variables
        protected double _startPrice;
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
        protected bool _isHardSLLastClosedPositionEntryPrice = false;
        protected bool _isBreakEvenStopLossActive = false;

        //Swordfish State Variables
        protected bool _isPendingOrdersClosed = false;
        protected bool _startPriceCaptured = false;
        protected bool _ordersPlaced = false;
        protected bool _secondSpikeSet = false;
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
            debugCSV.Add("NumberOfOrders," + NumberOfOrders.ToString());
            debugCSV.Add("Volume," + Volume.ToString());
            debugCSV.Add("VolumeMax," + VolumeMax.ToString());
            debugCSV.Add("OrderVolumeLevels," + OrderVolumeLevels.ToString());
            debugCSV.Add("VolumeMultipler," + VolumeMultipler.ToString());
            debugCSV.Add("TakeProfit," + TakeProfit.ToString());
            debugCSV.Add("ReducePositionRiskTime," + ReducePositionRiskTime.ToString());
            debugCSV.Add("retraceEnabled," + chaseEnabled.ToString());
            debugCSV.Add("retraceLevel1," + chaseLevel1.ToString());
            debugCSV.Add("retraceLevel2," + chaseLevel2.ToString());
            debugCSV.Add("retraceLevel3," + chaseLevel3.ToString());
            debugCSV.Add("FinalOrderStopLoss," + FinalOrderStopLoss.ToString());
            debugCSV.Add("HardStopLossBuffer," + HardStopLossBuffer.ToString());
            debugCSV.Add("TrailingStopPips," + TrailingStopPips.ToString());
            debugCSV.Add("--------------------------");

            debugCSV.Add("Label,Profit,Pips,EntryPrice,ClosePrice,SL,TP,Day,Date/Time," +
            "OpenedPositionsCount,ClosedPositionsCount,LastPositionEntryPrice,LastClosedPositionEntryPrice,LastProfitPrice," +
            "LastPositionLabel,DivideTrailingStopPips,isTrailingStopsActive,isBreakEvenStopLossActive," +
            "isHardSLLastClosedPositionEntryPrice,isHardSLLastPositionEntryPrice,isHardSLLastProfitPrice,StartPriceCaptured," +
            "OrdersPlaced,isReset,isTerminated,isReducedRiskTime");
            }

        protected string generateBotId()
        {
            Random randomIdGenerator = new Random();
            int id = randomIdGenerator.Next(0, 99999);
            return id.ToString("00000");
        }

        protected void setForSecondSpike()
        {
            if(!_secondSpikeSet)
            {
                _marketTimeInfo.open = new TimeSpan(16, 30, 0);
                _ordersPlaced = false;
            }
            _secondSpikeSet = true;
        }


        protected override void OnTick()
        {
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

                if (!_ordersPlaced)
                {
                    if(_secondSpikeSet)
                    {
                        //Place a second round of orders if the price is higher than the original entry point
                        if(Symbol.Ask > _startPrice)
                        {
                            placeSellOrders();
                        }

                    }
                    else 
                    {
                        placeSellOrders();
                        setForSecondSpike();
                    }

                }

                captureSpikePeak();
            }
            //It is outside Placing Trading Time
            else
            {
                if (_ordersPlaced)
                {
                    if (_openedPositionsCount - _closedPositionsCount > 0)
                    {
                        //As soon as in profit set all orders to breakeven

                        //If positions were opened at the end of swordfish time but have not recorded a spike peak
                        if (_spikePeakPips == 0)
                            captureSpikePeak();

                        //Look to reduce risk as Spike retraces
                        ManagePositionRisk();

                        //Positions still open after ReducePositionRiskTime
                        if (!_isReducedRiskTime && _marketTimeInfo.IsReduceRiskTime(IsBacktesting, Server.Time, ReducePositionRiskTime))
                        {
                            //Reduce Trailing Stop Loss by 50%
                            // DivideTrailingStopPips = 2;
                            _isReducedRiskTime = true;
                        }

                        //If trades still open at ClosingAllTime then take the hit and close remaining positions
                        if (!_isTerminated && _marketTimeInfo.IsCloseAllPositionsTime(IsBacktesting, Server.Time))
                        {
                            CloseAllPositions();
                            _isTerminated = true;
                        }
                    }
                    else
                    {
                        //No positions remain open and out of trading time
                        ResetSwordFish();
                    }

                    //Out of Trading time and all positions that were opened are now closed
                    if (_openedPositionsCount > 0 && _openedPositionsCount - _closedPositionsCount == 0)
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
            //Capture the highest point of the Spike within trading Time
            if (_openedPositionsCount > 0)
            {
                //We are selling - look for the lowest point
                if (Symbol.Bid < _spikePeakPrice || _spikePeakPrice == 0)
                {
                    _spikePeakPrice = Symbol.Bid;
                    _spikePeakPips = _startPrice -Symbol.Bid;
                }
            }
        }

        protected void setBreakEvensWithBuffer(double breakEvenTriggerPrice)
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

        // Place Sell Limit Orders
        protected void placeSellOrders()
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
                        entryPrice = 0,
                        label = _botId + "-" + getTimeStamp() + _marketTimeInfo.market + "-SWF#" + _orderCountLabel,
                        stopLossPips = FinalOrderStopLoss,
                        takeProfitPips = calcTakeProfit(OrderCount)
                    };
                    if (data == null)
                        continue;

                    //Place Market Orders immediately
                    ExecuteMarketOrderAsync(data.tradeType, data.symbol, data.volume, data.label + "X", data.stopLossPips, data.takeProfitPips, onTradeOperationComplete);
                    _orderCountLabel++;
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

        protected double calcTakeProfit(int orderCount)
        {
            double tp = 0;
            tp = TakeProfit * (1 / Symbol.TickSize) - orderCount;
            if (tp < 2)
                tp = MinTakeProfit * (1 / Symbol.TickSize);

            return tp;
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
              
                debugCSV.Add(args.Position.Label + 
                    "," + args.Position.GrossProfit + 
                    "," + args.Position.Pips + 
                    "," + args.Position.EntryPrice + 
                    "," + History.FindLast(args.Position.Label, Symbol, args.Position.TradeType).ClosingPrice + 
                    "," + args.Position.StopLoss + 
                    "," + args.Position.TakeProfit +
                    "," + Time.DayOfWeek + 
                    "," + Time + debugState());

                //Last position's SL has been triggered for a loss - NOT going down!
                if (_lastPositionLabel == args.Position.Label && args.Position.GrossProfit < 0)
                {
                    Print("CLOSING ALL POSITIONS due to furthest position losing");
                    CloseAllPositions();
                    _isTerminated = true;
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
                        setBreakEvensWithBuffer(_lastProfitPrice);
                    }
                }
            }
        }

        protected void ManagePositionRisk()
        {
            if (chaseEnabled)
            {
                //Calculate spike retrace factor
                double chaseFactor = calculateChaseFactor();

                if (_isReducedRiskTime)
                {
                    //reset HARD SL Limits with reduced SL's
                    _isHardSLLastPositionEntryPrice = true;

                    //Reduce all retrace limits
                    chaseLevel1 = chaseLevel1 / 2;
                    chaseLevel2 = chaseLevel2 / 2;
                    chaseLevel3 = chaseLevel3 / 2;
                }

                //Set hard stop losses as soon as Trading time is over
                if (!_isHardSLLastPositionEntryPrice && !IsTradingTime())
                {
                    setAllStopLossesWithBuffer(_lastPositionEntryPrice);
                    _isHardSLLastPositionEntryPrice = true;
                }



                //Set hard stop losses and activate Trail if Spike has retraced between than chaseLevel1 and chaseLevel2
                if (_isReducedRiskTime || (chaseLevel2 > chaseFactor && chaseFactor > chaseLevel1))
                {
                    //If Hard SL has not been set yet
                    if (!_isHardSLLastPositionEntryPrice && _lastPositionEntryPrice > 0)
                    {
                        setAllStopLossesWithBuffer(_lastPositionEntryPrice);
                        _isHardSLLastPositionEntryPrice = true;
                    }
                    //Active Breakeven Stop Losses
                    _isBreakEvenStopLossActive = true;
                }

                //Set harder SL and active BreakEven if it has retraced between than retraceLevel2 and retraceLevel3
                if (_isReducedRiskTime || (chaseLevel3 > chaseFactor && chaseFactor > chaseLevel2))
                {
                    //Set hard stop losses
                    if (!_isHardSLLastClosedPositionEntryPrice && _lastClosedPositionEntryPrice > 0)
                    {
                        setAllStopLossesWithBuffer(_lastClosedPositionEntryPrice);
                        _isHardSLLastClosedPositionEntryPrice = true;
                    }
                    //Activate Trailing Stop Losses
                    _isTrailingStopsActive = true;
                }

                //Set hardest SL if Spike retraced past retraceLevel3
                if (_isReducedRiskTime || chaseFactor > chaseLevel3)
                {
                    //Set hard stop losses
                    if (!_isHardSLLastProfitPrice && _lastProfitPrice > 0)
                    {
                        setAllStopLossesWithBuffer(_lastProfitPrice);
                        _isHardSLLastProfitPrice = true;
                    }
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

        protected void setAllStopLossesWithBuffer(double SLPrice)
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
            _isHardSLLastClosedPositionEntryPrice = false;
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
            state += "," + _isHardSLLastClosedPositionEntryPrice;
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
                    _marketTimeInfo.open = new TimeSpan(16, 26, 0);
                    // Market for swordfish trades closes at 8:05am.
                    _marketTimeInfo.close = new TimeSpan(16, 35, 0);
                    // Close all open Swordfish position at 11:29am before US opens.
                    _marketTimeInfo.closeAll = new TimeSpan(16, 35, 0);

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









