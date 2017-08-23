using System;
using Google.Protobuf.Collections;

namespace Niffler.Rules
{
    class ReduceRiskTimeReduceRetraceLevels : IRule
    {
        public ReduceRiskTimeReduceRetraceLevels(int priority) : base(priority) { }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        // If it is after Reduce Risk Time then reduce retrace levels by 50%
        override protected bool ExcuteRuleLogic()
        {

            if (BotState.IsAfterReducedRiskTime)
            {
                if(BotState.OrdersPlaced && BotState.PositionsRemainOpen())
                {
                    //Reduce all retrace limits
                    SpikeManager.ReduceLevelsBy50Percent();
                    ExecuteOnceOnly();
                    return true;
                }
            }
            return false;
        }

        // reset any botstate variables to the state prior to executing rule
        override protected void Reset()
        {

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
