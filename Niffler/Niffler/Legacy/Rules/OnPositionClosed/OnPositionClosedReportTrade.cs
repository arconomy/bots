using System;
using cAlgo.API;
using Google.Protobuf.Collections;

namespace Niffler.Rules
{
    class OnPositionClosedReportTrade : IPositionRule
    {
        public OnPositionClosedReportTrade(int priority) : base(priority) {}

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        //Report closing position trade
        override protected void Execute(Position position)
        {
            if (BotState.IsThisBotId(position.Label))
            {
                BotState.ClosedPositionsCount++;
                Reporter.ReportTrade(position,StopLossManager.GetStopLossStatus());
            }
        }

        // reset any botstate variables to the state prior to executing rule
        override protected void Reset()
        {
            BotState.ClosedPositionsCount = 0;
        }

        // report stats on rule execution 
        // e.g. execution rate, last position rule applied to, number of positions impacted by rule
        override public MapField<String, String> GetLastExecutionData()
        {
            return new MapField<string, string> { { "result", "success" } };

        }

        //create name of Rule Topic for Pub/Sub
        public override string GetPubSubTopicName()
        {
            return this.GetType().Name;
        }
    }
}
