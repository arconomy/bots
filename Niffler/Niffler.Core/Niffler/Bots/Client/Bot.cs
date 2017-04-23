using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo.API; 
using cAlgo.API.Internals;

namespace Niffler.Bots.Client

{
    public class Bot : cAlgo.API.Robot
    {

        public virtual double TestParameter { get; set; }

        protected override void OnStart()
        {

            Print("and the param is: " + TestParameter.ToString());
           
            Positions.Opened += PositionsOpened;
            Positions.Closed += PositionsClosed;

            Timer.Start(new TimeSpan(0, 0, 10));

            base.OnStart();
        }

        private void PositionsOpened(PositionOpenedEventArgs obj)
        {
            throw new NotImplementedException();
        }

        private void PositionsClosed(PositionClosedEventArgs obj)
        {
            throw new NotImplementedException();
        }

        protected override void OnTimer()
        {

            Print("Called from Niffle.. oooh!r: " + MarketSeries.Close.ToString());

            PlaceLimitOrder(TradeType.Buy, Symbol, 1, 2000);


            base.OnTimer();
        }

        protected override void OnBar()
        {
            base.OnBar();
        }


        protected override void OnTick()
        {
            base.OnTick();
        }


        protected override void OnStop()
        {
            base.OnStop();
        }



    }

}

     