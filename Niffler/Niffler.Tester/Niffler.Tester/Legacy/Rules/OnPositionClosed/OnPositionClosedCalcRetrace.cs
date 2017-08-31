using System;
using System.Collections.Generic;
using cAlgo.API;
using Niffler.Messaging.RabbitMQ;
using Niffler.Strategy;
using Niffler.Rules;
using Niffler.Messaging.Protobuf;
using Niffler.Rules.TradingPeriods;

namespace Niffler.Rules
{
    class OnPositionClosedCalcRetrace : IRule
    {
        enum Spike
        {
            NONE = 0,
            UP = 1,
            DOWN = 2
        }
        private string StrategyId;
        private double OpenPrice; //Price that the strategy opened not the market
        private double SpikeUpPeakPips;
        private double SpikeUpPeakPrice;
        private double SpikeDownPeakPips;
        private double SpikeDownPeakPrice;
        private double MinSpikePips;
        private Spike SpikeDirection = Spike.NONE;

        public int Level1 { get; set; }
        public int Level2 { get; set; }
        public int Level3 { get; set; }
        private int RetraceFactor { get; set; }

        //Spike Manager captures the Tick data for the duration of the strategy and tries to identify a profile for the trading data
        public OnPositionClosedCalcRetrace(IDictionary<string, string> botConfig, RuleConfiguration ruleConfig) : base(botConfig, ruleConfig)
        {
            botConfig.TryGetValue(StrategyConfiguration.STRATEGYID, out string StrategyId);
        }

        public override object Clone()
        {
            return new OnPositionClosedCalcRetrace(StrategyConfig,RuleConfig);
        }

        public override bool Init()
        {
            bool initSuccess = false;
            //Initialised required config values
            if (RuleConfig.Params.TryGetValue(RuleConfiguration.MINSPIKEPIPS, out object minSpikePips))
            {
                if (Double.TryParse(minSpikePips.ToString(), out MinSpikePips)) initSuccess = true;
            }
            
            //Initialised inactive
            IsActive = false;

            //Reset all variables
            OpenPrice = 0;
            SpikeUpPeakPips = 0;
            SpikeUpPeakPrice = 0;
            SpikeDownPeakPips = 0;
            SpikeDownPeakPrice = 0;
            SpikeDirection = Spike.NONE;

            return initSuccess;
        }

        protected override string GetServiceName()
        {
            return nameof(OnTickCaptureSpike);
        }

        protected override bool ExcuteRuleLogic(Niffle message)
        {
            if (IsOnPositionsMessageEmpty(message)) return false;

            //Calculate retrace
            CalculateRetraceFactor(message.Positions);

            return true;
        }

        private double GetMidPrice(Tick tick)
        {
            return tick.Ask + tick.Spread / 2;
        }

        protected override List<RoutingKey> SetListeningRoutingKeys()
        {
            return RoutingKey.Create(Source.WILDCARD, Messaging.RabbitMQ.Action.WILDCARD, Event.ONTICK).ToList();
        }

        protected override void OnServiceNotify(Niffle message, RoutingKey routingKey)
        {
            if (IsServiceMessageEmpty(message)) return;

            //Listening for OpenForTrading notification to activate
            if (routingKey.Source == nameof(OnOpenForTrading) && message.Service.Success)
            {
                IsActive = true;
            }
        }

        protected override void OnStateUpdate(Niffle message, RoutingKey routingKey)
        {
            throw new NotImplementedException();
        }

       
        //Return the greater retrace of the percentage price or percent closed positions
        public void CalculateRetraceFactor(Messaging.Protobuf.Positions positions)
        {
            double retraceFactor = 0;
            double percentClosed = BotState.CalcPercentOfPositionsClosed();
            if (percentClosed <= percentRetrace)
            {
                retraceFactor = percentRetrace;
            }
            else
            {
                retraceFactor = percentClosed;
            }
            RetraceFactor = (int) retraceFactor;
        }

       
    }
}
