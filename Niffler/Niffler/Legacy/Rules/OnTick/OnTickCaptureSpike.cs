using System;
using System.Collections.Generic;
using Niffler.Messaging.RabbitMQ;
using Niffler.Strategy;
using Niffler.Messaging.Protobuf;
using Niffler.Rules.TradingPeriods;

namespace Niffler.Rules
{
    class OnTickCaptureSpike : IRule
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
        public OnTickCaptureSpike(IDictionary<string, string> botConfig, RuleConfiguration ruleConfig) : base(botConfig, ruleConfig)
        {
            botConfig.TryGetValue(StrategyConfiguration.STRATEGYID, out string StrategyId);
        }

        public override object Clone()
        {
            return new OnTickCaptureSpike(StrategyConfig,RuleConfig);
        }

        public override bool Init()
        {
            bool initSuccess = false;
            //Initialised required config values - Minimum Spike Pips values determines the pip distance required to indicate a spike in either direction
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
            if (IsTickMessageEmpty(message)) return false;

            //Capture price
            if(OpenPrice == 0)
            {
                OpenPrice = GetMidPrice(message.Tick);
            }

            //Capture spike peak
            CaptureSpike(message.Tick);

            //Calculate retrace
            CalculatePercentageRetrace(message.Tick);

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

        //Capture Spike
        public void CaptureSpike(Tick tick)
        {
            double tickMidPrice = GetMidPrice(tick);
            //If the Mid price is above the OpenPrice then update the Spike UP
            if (tickMidPrice > OpenPrice)
            {
                if (tickMidPrice > SpikeUpPeakPrice || SpikeUpPeakPrice == 0)
                {
                    SpikeUpPeakPrice = tickMidPrice;
                    SpikeUpPeakPips = tickMidPrice - OpenPrice;
                    if (SpikeUpPeakPips > MinSpikePips) SpikeDirection = Spike.UP;
                }
            }

            //If the Mid price is below the OpenPrice then update the Spike DOWN
            if (tickMidPrice < OpenPrice)
            {
                if (tickMidPrice < SpikeDownPeakPrice || SpikeDownPeakPrice == 0)
                {
                    SpikeDownPeakPrice = tickMidPrice;
                    SpikeDownPeakPips = OpenPrice - tickMidPrice;
                    if (SpikeUpPeakPips > MinSpikePips) SpikeDirection = Spike.DOWN;
                }
            }
        }

        protected void CalculatePercentageRetrace(Tick tick)
        {
            double percentRetrace = 0;
            double tickMidPrice = GetMidPrice(tick);
            if (SpikeDirection == Spike.UP)
            {
                percentRetrace = ((tickMidPrice - OpenPrice) / (SpikeUpPeakPrice - OpenPrice))*100;

                PublishStateUpdate("SpikeUpRetracePercent", percentRetrace);
            }

            if (SpikeDirection == Spike.DOWN)
            {
                percentRetrace = ((OpenPrice - tick.Ask) / (OpenPrice - SpikeDownPeakPrice))*100;
                PublishStateUpdate("SpikeDownRetracePercent", percentRetrace);
            }
        }
    }
}
