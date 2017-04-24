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

        public virtual string TraderName { get; set; }

        public virtual string TraderEmail { get; set; }

        protected override void OnStart()
        {

            Print("Starting Niffler Client Bot");

            Positions.Opened += PositionsOpened;
            Positions.Closed += PositionsClosed;

            Timer.Start(new TimeSpan(0, 0, 10));


            //Check and Update Account Status
            Business.Accounts.Update(Account, TraderName, TraderEmail);

            base.OnStart();
        }

        private void PositionsOpened(PositionOpenedEventArgs obj)
        {
            if (!IsBacktesting)
                Business.Positions.Update(Account, obj.Position, "Opened");
        }

        private void PositionsClosed(PositionClosedEventArgs obj)
        {
            if (!IsBacktesting)
                Business.Positions.Update(Account, obj.Position, "Closed");
        }

        protected override void OnTimer()
        {

            Print("Niffler Updating Market Stats");

            // PlaceLimitOrder(TradeType.Buy, Symbol, 1, 2000);
            Business.Accounts.Update(Account, TraderName, TraderEmail);

            if (!IsBacktesting) 
                    Business.Positions.UpdatePositions (Account, Positions  );

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
            Print("Stopped Niffler Client Bot.");
            base.OnStop();
        }








    }

}

