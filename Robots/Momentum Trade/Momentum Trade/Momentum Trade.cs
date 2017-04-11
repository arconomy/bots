using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MomentumTrade : Robot
    {

        [Parameter()]
        public DataSeries SourceSeries { get; set; }


        [Parameter("Default Contract Size", DefaultValue = 2)]
        public int DefaultContractSize { get; set; }


        [Parameter("Moving Average Type", DefaultValue = MovingAverageType.Exponential)]
        public MovingAverageType MAType { get; set; }

        [Parameter("Min. MA Gap", DefaultValue = 1.5)]
        public double MinMAGap { get; set; }

        [Parameter("Trailing Stop (points)", DefaultValue = 5.0)]
        public double StopLoss { get; set; }

        [Parameter("Pending Order Expiration. (Mins)", DefaultValue = 9.0)]
        public double PendingOrderEpiration { get; set; }

        [Parameter("Min. Signal Candle Length", DefaultValue = 2.0)]
        public double MinSignalLength { get; set; }


        // Signal Movement
        [Parameter("Signal Bars Back", DefaultValue = 1)]
        public int SignalBarsBack { get; set; }

        [Parameter("-- Min. Signal EMA Movement", DefaultValue = 0.5)]
        public double MinSignalEMAMovement { get; set; }

        [Parameter("-- Max. Signal EMA Movement", DefaultValue = 1000)]
        public double MaxSignalEMAMovement { get; set; }


        // Trend Movement

        [Parameter("Trend EMA Bars Back", DefaultValue = 10)]
        public int TrendBarsBack { get; set; }

        [Parameter("-- Min. Trend EMA Min. Movement", DefaultValue = 5.0)]
        public double MinTrendEMAMovement { get; set; }

        [Parameter("-- Min. Trend EMA  Max. Movement", DefaultValue = 1000)]
        public double MaxTrendEMAMovement { get; set; }



        public MovingAverage MA20;
        public MovingAverage MA27;
        public MovingAverage MA34;


        //Hashtable OrderAttributes = new Hashtable();

        protected override void OnStart()
        {
            // Put your initialization logic here


        }




        protected override void OnBar()
        {

            // This Bar is a potential Trigger Bar.  
            MA20 = Indicators.MovingAverage(SourceSeries, 20, MAType);
            MA34 = Indicators.MovingAverage(SourceSeries, 34, MAType);
            MA27 = Indicators.MovingAverage(SourceSeries, 27, MAType);
            DateTime EntryExpiration = MarketSeries.OpenTime.LastValue.AddMinutes(PendingOrderEpiration);


            // Only run during Market Opening Times.. 
            if (MarketSeries.OpenTime.LastValue.TimeOfDay <= new TimeSpan(8, 0, 0) & MarketSeries.OpenTime.LastValue.TimeOfDay >= new TimeSpan(11, 0, 0))
            {
                return;
            }


            // Reasons not to take the trade:
            if ((MarketSeries.High.Last(1) - MarketSeries.Low.Last(1)) > 50)
            {
                Print("Signal Bar greater than 50 points, exiting...");
                return;
            }

            // Reasons not to take the trade:
            if ((MarketSeries.High.Last(1) - MarketSeries.Low.Last(1)) < MinSignalLength)
            {
                Print("Signal Bar too small, exiting...");
                return;
            }

            // Reasons not to take the trade:
            if (IsPreviousBarsOK() == false)
            {
                Print("Previous Bars fail");
                return;
            }


            // Only look for trades if there are no existing positions open.
            if (Positions.Count == 0)
            {

                //  Only trade if there is a sufficient Gap between MA's
                double MAGap = Math.Abs(MA20.Result.Last(1) - MA34.Result.Last(1));
                if (MAGap >= MinMAGap)
                {


                    // Only trade if Candle is touching the MA lines 
                    if (DoesCandleTouch(MarketSeries.High.Last(1), MarketSeries.Low.Last(1), MA20.Result.Last(1), MA34.Result.Last(1)) == true)
                    {
                        //Get Direction Of Signal Bar
                        TradeType SignalBarDirection = TradeType.Sell;
                        if (MarketSeries.Open.Last(1) < MarketSeries.Close.Last(1))
                        {
                            SignalBarDirection = TradeType.Buy;

                        }
                        else
                        {
                            SignalBarDirection = TradeType.Sell;
                        }

                        Print("Bar Direction: " + SignalBarDirection.ToString());

                        if (IsCloseWithinPercent(MarketSeries.High.Last(1), MarketSeries.Low.Last(1), SignalBarDirection, MarketSeries.Close.Last(1)) == true)
                        {
                            // Rule 10: Make sure the candle is in the same direction as the trend

                            // string Trend = GetMACDTrend();
                            string Trend = GetMA27Trend();


                            // Check for Minimum Trend Movement
                            bool MinSignal = CheckMovment(SignalBarsBack, MinSignalEMAMovement, MaxSignalEMAMovement, "Signal Movement Result:");

                            if (!MinSignal)
                            {
                                Print("Signal movement was not sufficient.");
                                return;
                            }

                            // Check for Minimum Trend Movement
                            bool MinTrend = CheckMovment(TrendBarsBack, MinTrendEMAMovement, MaxTrendEMAMovement, "Trend Movement Result:");

                            if (!MinTrend)
                            {
                                Print("Trned movement was not sufficient.");
                                return;
                            }



                            if (Trend == "UP" & SignalBarDirection == TradeType.Buy)
                            {
                                double EntryPrice = MarketSeries.High.Last(1) + 2;
                                double InitialStopLoss = GetStopLoss(SignalBarDirection, EntryPrice);

                                TradeResult CurrentOrder = PlaceLimitOrder(SignalBarDirection, Symbol, DefaultContractSize, EntryPrice, "Woo!", InitialStopLoss, InitialStopLoss, EntryExpiration);

                                if (CurrentOrder.IsSuccessful)
                                    Print("Buy Successful!");
                                else
                                    Print("Buy Error: " + CurrentOrder.Error.Value.ToString());

                            }




                            if (Trend == "DOWN" & SignalBarDirection == TradeType.Sell)
                            {


                                double EntryPrice = MarketSeries.Low.Last(1) - 2;
                                double InitialStopLoss = GetStopLoss(SignalBarDirection, EntryPrice);

                                TradeResult CurrentOrder = PlaceLimitOrder(SignalBarDirection, Symbol, DefaultContractSize, EntryPrice, "Woo!", InitialStopLoss, InitialStopLoss, EntryExpiration);

                                if (CurrentOrder.IsSuccessful)
                                    Print("Buy Successful!");
                                else
                                    Print("Buy Error: " + CurrentOrder.Error.Value.ToString());
                            }





                        }

                        else
                        {
                            Print("Signal didnt close within 25% of Close.");
                        }


                    }
                    else
                    {
                        Print("Cannot trade due to Candles dont touch");
                    }



                }
                else
                {
                    Print("Cannot trade due to EMA Gap being too small");
                }


            }
            else
            {
                Print("Cannot trade due to Open Position");
            }


        }




        protected override void OnTick()
        {



            // OnTick manages and re-evaluates the Account Positions


            foreach (var position in Positions)
            {

                continue;
                //disbale!

                if (Symbol.Code != position.SymbolCode)
                    continue;


                //tralling stop
                if (position.GrossProfit <= 0)
                    continue;


                //Get Take Profit
                double TPPips = ((double)position.EntryPrice - (double)position.StopLoss);
                double TakeProfitPips = Math.Abs(TPPips);

                // Close off a portion of the trade if targets are met.

                if (((position.EntryPrice + TakeProfitPips) > Symbol.Bid) & position.TradeType == TradeType.Sell)
                {
                    if (position.Volume == DefaultContractSize)
                    {
//
                        //  ClosePosition(position, RunningContractSize);
                        //     ModifyPosition(position, position.EntryPrice, position.TakeProfit - 10);
                    }

                }

                if (((position.EntryPrice + TakeProfitPips) > Symbol.Ask) & position.TradeType == TradeType.Buy)
                {
                    if (position.Volume == DefaultContractSize)
                    {
                        // ClosePosition(position, RunningContractSize);
                        // ModifyPosition(position, position.EntryPrice, position.TakeProfit + 10);
                    }
                }




            }




        }

        protected override void OnStop()
        {


            // Put your deinitialization logic here
        }










        //-----------------------------------------------------------------------------------------------
        //
        // Helper Functions / To be moved to DLL.
        //
        //-----------------------------------------------------------------------------------------------


        //  Rule 10: 
        //    Signal bar must touch 34 or 20 EMA and then close within 25% of the highest/lowest point 
        //    of the bar (depending on direction – see rule 8 and 3 for direction rules)

        protected bool DoesCandleTouch(double High, double Low, double MA20, double MA34)
        {

            // if the candle straddles the EMA's
            if ((High >= MA20 | High >= MA34) & (Low <= MA20 | Low <= MA34))
                return true;

            // if the cande top is within the EMA
            if ((High >= MA20 & High <= MA34) | (High >= MA34 & High <= MA20))
                return true;

            // if the candle bottom is within the EMD
            if ((Low >= MA20 & Low <= MA34) | (Low >= MA34 & Low <= MA20))
                return true;


            // must be inside
            return false;

        }

        protected bool IsCloseWithinPercent(double High, double Low, TradeType Direction, double Close)
        {

            double BarLength = (High - Low);

            if (Close >= (High - (BarLength * 0.25)) & Direction == TradeType.Buy)
                return true;

            if (Close <= (Low + (BarLength * 0.25)) & Direction == TradeType.Sell)
                return true;


            return false;

        }



        protected bool IsPreviousBarsOK()
        {


            int IsTouching = 0;
            MovingAverage MA20 = Indicators.MovingAverage(SourceSeries, 20, MAType);
            MovingAverage MA34 = Indicators.MovingAverage(SourceSeries, 34, MAType);


            for (int i = 2; i <= 5; i++)
            {

                if (DoesCandleTouch(MarketSeries.High.Last(i), MarketSeries.Low.Last(i), MA20.Result.Last(i), MA34.Result.Last(i)) == true)
                    IsTouching += 1;

            }



            if (IsTouching > 0)
                return true;
            else
                return false;

        }




        // doenst seem to offer good results...
        protected string GetMA27Trend()
        {

            MovingAverage MA27 = Indicators.MovingAverage(SourceSeries, 27, MAType);

            double TestPoint1 = MA27.Result.Last(3) - MA27.Result.Last(6);
            double TestPoint2 = MA27.Result.Last(3) - MA27.Result.Last(9);

            if (TestPoint1 > 1 & TestPoint2 > TestPoint1)
                return "UP";

            if (TestPoint1 < -1 & TestPoint2 < TestPoint1)
                return "DOWN";


            return "NETURAL";


        }


        // Get Current Trend
        protected bool CheckMovment(int BarsBack, double MinMovement, double MaxMovement, string Description)
        {

            MovingAverage MA27 = Indicators.MovingAverage(SourceSeries, 27, MAType);

            double SignalPoint = MA27.Result.Last(1);

            double CheckPoint = MA27.Result.Last(BarsBack + 1);

            double Result = Math.Abs(SignalPoint - CheckPoint);

            Print(Description + " " + Result.ToString());

            if (Result <= MinMovement | Result >= MaxMovement)
                return false;
            else
                return true;



        }



        protected double GetStopLoss(TradeType Direction, double EntryPrice)
        {



            bool IsBigMove = false;
            // missing signal candle so its 1 to 8 candles back
            for (int i = 1; i <= 8; i++)
            {

                if (MarketSeries.High.Last(i) > (MarketSeries.Low.Last(i) + 50))
                    IsBigMove = true;

            }

            int CandlesBack = 7;
            if (IsBigMove)
                CandlesBack += 1;


            double Highest = int.MinValue;
            double Lowest = int.MaxValue;
            for (int i = 1; i <= CandlesBack; i++)
            {


                if (MarketSeries.High.Last(i) > Highest)
                    Highest = MarketSeries.High.Last(i);

                if (MarketSeries.Low.Last(i) < Lowest)
                    Lowest = MarketSeries.Low.Last(i);

            }





            if (Direction == TradeType.Buy)
                return (Math.Abs(EntryPrice - Lowest) - 1);

            if (Direction == TradeType.Sell)
                return (Math.Abs(EntryPrice - Highest) + 1);

            return 0.0;

        }






    }
}
