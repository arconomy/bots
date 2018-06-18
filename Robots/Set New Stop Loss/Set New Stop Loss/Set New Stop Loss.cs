using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class SetNewStopLoss : Robot
    {
        [Parameter(DefaultValue = 0.0)]
        public double StopLossPrice { get; set; }

        protected override void OnStart()
        {
            foreach (Position p in Positions)
            {
                try
                {
                    ModifyPositionAsync(p, StopLossPrice, p.TakeProfit, OnModifyHardSLComplete);
                } catch (Exception e)
                {
                    Print("Failed to Modify Position: " + e.Message);
                }
            }
        }

        protected void OnModifyHardSLComplete(TradeResult tr)
        {
            OnTradeOperationComplete(tr, "FAILED to modify HARD stop loss: ");
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
