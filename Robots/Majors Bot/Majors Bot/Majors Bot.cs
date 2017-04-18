using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MajorsBot : Robot
    {
        [Parameter("Modify Period", DefaultValue = 2)]
        public int ModifyPeriod { get; set; }

        [Parameter("Stop Loss", DefaultValue = 30)]
        public int StopLoss { get; set; }

        [Parameter("Take Profit", DefaultValue = 30)]
        public int TakeProfit { get; set; }

        [Parameter("Entry Offset", DefaultValue = 30)]
        public int EntryOffset { get; set; }

        [Parameter("Volume", DefaultValue = 10)]
        public int Volume { get; set; }


        protected override void OnStart()
        {
            Positions.Opened += PositionsOnOpened;
            Positions.Closed += PositionsOnClosed;
            Timer.Start(ModifyPeriod);
        }


        protected override void OnTimer()
        {

            // Create new Pending Orders if they dont already exist.... 
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


                //Modify Order
                if (!HasSellOrder)
                    PlaceStopOrder(TradeType.Sell, Symbol, Volume, (Symbol.Bid - EntryOffset), "SELL", StopLoss, TakeProfit);


            }



        }

        protected override void OnTick()
        {
            // Put your core logic here




        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}


using System;
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

        [Parameter("# Positions closed by retrace to trigger setting trailing SL", DefaultValue = 40)]
        public double PercentageOfPositionsClosed { get; set; }

        [Parameter("Mins after swordfish period to reduce position risk", DefaultValue = 45)]
        public int ReducePositionRiskTime { get; set; }

        [Parameter("Initial SL for last Order placed", DefaultValue = 2)]
        public double FinalOrderStopLoss { get; set; }

        [Parameter("Triggered SL buffer from Last Position Entry", DefaultValue = 2)]
        public double StopLossFromEntryBuffer { get; set; }

        [Parameter("Trailing SL fixed distance", DefaultValue = 10)]
        public double TrailingStopPips { get; set; }

        protected MarketTimeInfo swordFishTimeInfo;

        protected double OpenPrice;
        protected BollingerBands Boli;
        protected TradeType LastPositionTradeType;
        protected double LastPositionEntryPrice;
        protected string LastPositionLabel;

        protected int OpenedPositionsCount = 0;
        protected int ClosedPositionsCount = 0;   
        protected double LastProfitPrice;
        protected double DivideTrailingStopPips = 1;

        protected bool isTrailingStopsActive = false;
        protected bool isAllPositionsHardSLSet = false;
        protected bool PositionRiskManaged = false;
        protected bool OpenPriceCaptured = false;
        protected bool OrdersPlaced = false;
        protected bool PositionRiskReduced50 = false;
        protected bool PositionRiskReduced66 = false;
        protected bool isSwordfishTerminated = false;
        protected bool isSwordFishReset = false;
        protected bool isBreakEvenStopLossActive = false;
        
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
                        //If Spike retrace has not occurred (in PositionsOnClosed) then ManagePositionRisk
                        if (!PositionRiskManaged)
                        {
                            Print("---- MANAGING POSITIONS out of Swordfish, No Retrace: ", Time);
                            ManagePositionRisk(LastPositionEntryPrice);
                        }

                        //Positions still open after ReducePositionRiskTime and spike not retraced
                        if (swordFishTimeInfo.IsReduceRiskTime(IsBacktesting, Server.Time, ReducePositionRiskTime) && !PositionRiskReduced50)
                        {
                            Print("---- REDUCE POSITION RISK 66% out of Swordfish CLOSING TIME, With Positions Managed: ", Time);
                            //Reduce Trailing Stop by 50%
                            DivideTrailingStopPips = 2;
                            PositionRiskReduced50 = true;
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
                    double newStopLoss;
                    foreach (Position _p in Positions)
                    {

                        bool isProtected = _p.StopLoss.HasValue;
                        if (isProtected)
                        {
                            newStopLoss = (double)_p.StopLoss;
                        }
                        else
                        {
                            if (LastProfitPrice != 0)
                                newStopLoss = LastProfitPrice;
                            else
                                newStopLoss = LastPositionEntryPrice;
                        }

                        if (_p.TradeType == TradeType.Buy)
                        {
                            newStopLoss = Symbol.Ask - TrailingStopPips / DivideTrailingStopPips;
                            if (newStopLoss > Symbol.Ask)
                                return;
                            if (newStopLoss - _p.StopLoss < Symbol.TickSize)
                                return;
                        }

                        if (_p.TradeType == TradeType.Sell)
                        {
                            newStopLoss = Symbol.Bid + TrailingStopPips / DivideTrailingStopPips;
                            if (newStopLoss < Symbol.Bid)
                                return;
                            if (_p.StopLoss - newStopLoss < Symbol.TickSize)
                                return;
                        }

                        TradeResult tr = ModifyPosition(_p, newStopLoss, _p.TakeProfit);
                        if (!tr.IsSuccessful)
                            debug("FAILED to modify SL", tr);

                    
                }

            }

        }

        protected void ManagePositionRisk(double AllPositionsHardSL)
        {
            //Close any positions that have not been triggered
            CloseAllPendingOrders();

            //Set stop losses
            SetAllStopLosses(AllPositionsHardSL);

            //Active Breakeven Stop Losses
            isBreakEvenStopLossActive = true;

            //Activate Trailing Stop Losses
            isTrailingStopsActive = true;

            //Only manage Positions flag
            PositionRiskManaged = true;
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
                //Find out trade type
                if (args.Position.TradeType == TradeType.Buy)
                    LastProfitPrice = Symbol.Ask;
                if (args.Position.TradeType == TradeType.Sell)
                    LastProfitPrice = Symbol.Bid;

                //There could be a second spike so don't cancel all pending orders.
                if (!isAllPositionsHardSLSet)
                    SetAllStopLosses(LastPositionEntryPrice);

                //If the spike has retraced then close all pending and set trailing stop
                if (HasSpikeRetraced())
                {
                    //We are taking profit now
                    CloseAllPendingOrders();
                    
                    //Trail remaining positions from the last winning position Take Profit rather from the furthest position entry point
                    if (args.Position.TradeType == TradeType.Buy)
                        LastProfitPrice = Symbol.Ask;
                    if (args.Position.TradeType == TradeType.Sell)
                        LastProfitPrice = Symbol.Bid;

                    Print("---- MANAGING POSITIONS in POSITION CLOSED, With Retrace ----");
                    ManagePositionRisk(LastProfitPrice);
                }

                // Set any outstanding positions to break even if possible
                if (isBreakEvenStopLossActive)
                    setBreakEvens((double) args.Position.TakeProfit, args.Position.TradeType);
            }
        }

        protected void setBreakEvens(double LastTakeProfitPrice, TradeType _tradeType)
        {
            TradeResult tr;
            foreach(Position _p in Positions)
            {
                if (_tradeType == TradeType.Buy)
                    {
                        if(LastTakeProfitPrice > _p.EntryPrice)
                        {
                            Print("---- Modifying SL to BREAKEVEN ----", _p.Label);
                            tr = ModifyPosition(_p, _p.EntryPrice, _p.TakeProfit);
                            if (!tr.IsSuccessful)
                                debug("FAILED to modify", tr);
                        }
                        
                    }
                if (_tradeType == TradeType.Sell)
                {
                        if(LastTakeProfitPrice < _p.EntryPrice)
                        {
                            Print("---- Modifying SL to BREAKEVEN ----", _p.Label);
                            tr = ModifyPosition(_p, _p.EntryPrice, _p.TakeProfit);
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
            LastPositionEntryPrice = 0;
            LastProfitPrice = 0;
            LastPositionLabel = "NO LAST POSITION SET";

            //reset risk management variables
            PositionRiskReduced50 = false;
            PositionRiskReduced66 = false;
            isSwordfishTerminated = false;
            DivideTrailingStopPips = 1;
            isTrailingStopsActive = false;
            PositionRiskManaged = false;
            isBreakEvenStopLossActive = false;
            isAllPositionsHardSLSet = false;

            //reset swordfish bot state variables
            OpenPriceCaptured = false;
            OrdersPlaced = false;


            isSwordFishReset = true;
        }




        protected void SetAllStopLosses(double SL)
        {
            switch (LastPositionTradeType)
            {
                case TradeType.Buy:
                    SetStopLossForAllPositions(SL - StopLossFromEntryBuffer);
                    break;
                case TradeType.Sell:
                    SetStopLossForAllPositions(SL + StopLossFromEntryBuffer);
                    break;
            }
            isAllPositionsHardSLSet = true;
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

    }

}











