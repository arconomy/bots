﻿using System;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using Niffler.Messaging.Protobuf;
using Niffler.Messaging.RabbitMQ;

namespace Niffler.Rules
{
    class ReduceRiskTimeSetHardSLToLastProfitPositionCloseWithBuffer : IRule
    {
        public ReduceRiskTimeSetHardSLToLastProfitPositionCloseWithBuffer(int priority) : base(priority) { }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        //If after reduce risk time then set hard stop losses to Last Profit Positions Entry Price with buffer
        override protected bool ExcuteRuleLogic()
        {
            if (BotState.IsAfterReducedRiskTime)
            {
                if (BotState.OrdersPlaced && BotState.PositionsRemainOpen())
                {
                    //If Hard SL has not been set yet
                    if (BotState.LastProfitPositionClosePrice > 0)
                    {
                        StopLossManager.SetSLWithBufferForAllPositions(BotState.LastProfitPositionClosePrice);
                        return true;
                    }
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

        protected override string GetServiceName()
        {
            throw new NotImplementedException();
        }

        protected override bool ExcuteRuleLogic(Niffle message)
        {
            throw new NotImplementedException();
        }

        protected override List<RoutingKey> SetListeningRoutingKeys()
        {
            throw new NotImplementedException();
        }

        protected override void OnServiceNotify(Niffle message, RoutingKey routingKey)
        {
            throw new NotImplementedException();
        }

        protected override void OnStateUpdate(Niffle message, RoutingKey routingKey)
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public override bool Init()
        {
            throw new NotImplementedException();
        }
    }
}
