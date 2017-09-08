using Niffler.Messaging.RabbitMQ;
using System.Collections.Generic;
using Niffler.Messaging.Protobuf;
using Niffler.Core.Config;
using static Niffler.Data.State;
using Niffler.Common;
using Niffler.Rules.TradingPeriods;

namespace Niffler.Rules.Capture
{
    class CaptureSpike : IRule
    {
        private double SpikeStartPrice = 0;
        private double SpikeUpPeakPips = 0;
        private double SpikeDownPeakPips = 0;
        private bool isSpikeDirectionSet = false;
        private int MinSpikePips;
        private SpikeDirection SpikeDirection = SpikeDirection.NONE;

        public CaptureSpike(StrategyConfiguration StrategyConfig, RuleConfiguration ruleConfig) : base(StrategyConfig, ruleConfig) { }

        //Retreive data from config to initialise the rule
        public override void Init()
        {
            if (RuleConfig.Params.TryGetValue(RuleConfiguration.MINSPIKEPIPS, out object minSpikePips))
            {
                if (!int.TryParse(minSpikePips.ToString(), out MinSpikePips)) IsInitialised = false;
            }

            //Activate on OpenForTrading notification
            IsActive = false;
        }

        //Execute rule logic
        override protected bool ExcuteRuleLogic(Niffle message)
        {
            if (IsTickMessageEmpty(message)) return false;
            if (SpikeStartPrice == 0) return false; //If the Spike Start price had not been set then exit

            bool notifyOfSpikePeakCapture = false;
            if(SetSpikeUpPeak(Utils.GetMidPrice(message.Tick)))
            {
                PublishStateUpdate(Data.State.SPIKEUPPEAK, SpikeUpPeakPips);
                notifyOfSpikePeakCapture = true;
            }

            if (SetSpikeDownPeak(Utils.GetMidPrice(message.Tick)))
            {
                PublishStateUpdate(Data.State.SPIKEDOWNPEAK, SpikeDownPeakPips);
                notifyOfSpikePeakCapture = true;
            }

            if(SetSpikeDirection())
            {
                PublishStateUpdate(Data.State.SPIKEDIRECTION, (int)SpikeDirection);
                notifyOfSpikePeakCapture = true;
            }
            return notifyOfSpikePeakCapture;
        }

        private void SetSpikeStart(double startPrice)
        {
            if (SpikeStartPrice == 0)
            {
                SpikeStartPrice = startPrice;
            }
        }

        private bool SetSpikeUpPeak(double midPrice)
        {
            if(midPrice > SpikeStartPrice + SpikeUpPeakPips)
            {
                SpikeUpPeakPips = midPrice - SpikeStartPrice;
                return true;
            }
            return false;
        }

        private bool SetSpikeDownPeak(double midPrice)
        {
            if (midPrice < SpikeStartPrice - SpikeDownPeakPips)
            {
                SpikeDownPeakPips = SpikeStartPrice - midPrice;
                return true;
            }
            return false;
        }

        private bool SetSpikeDirection()
        {
            if (isSpikeDirectionSet) return false;

            if (SpikeUpPeakPips > MinSpikePips)
            {
                SpikeDirection = SpikeDirection.UP;
                isSpikeDirectionSet = true;
            }
            
            if(SpikeDownPeakPips > MinSpikePips)
            {
                SpikeDirection = SpikeDirection.DOWN;
                isSpikeDirectionSet = true;
            }
            return isSpikeDirectionSet;
        }


        override protected void OnServiceNotify(Niffle message, RoutingKey routingKey)
        {
            if (IsServiceMessageEmpty(message)) return;

            //Listening for OpenForTrading notification to activate
            if (routingKey.Source == nameof(OnOpenForTrading) && message.Service.Success)
            {
                IsActive = true;
            }

            //Listening for OnCloseForTrading notification to deactivate
            if (routingKey.Source == nameof(OnCloseForTrading) && message.Service.Success)
            {
                IsActive = false;
            }
        }

        protected override void OnStateUpdate(Niffle message, RoutingKey routingKey)
        {
            //Listen to updateState msg from OpenTrading Service
            if (routingKey.Source == nameof(OnOpenForTrading))
            {
                if (message.State.Key == Data.State.OPENPRICE && message.State.ValueType == Messaging.Protobuf.State.Types.ValueType.Double)
                {
                    SetSpikeStart(message.State.DoubleValue);
                }
            }
        }

        protected override string GetServiceName()
        {
            return nameof(CaptureSpike);
        }

        protected override List<RoutingKey> SetListeningRoutingKeys()
        {
            //Listen for OnTick
            List<RoutingKey> routingKeys = RoutingKey.Create(Source.WILDCARD, Messaging.RabbitMQ.Action.WILDCARD, Event.ONTICK).ToList();

            //Listen for successful Service execution Notification from OnOpenForTrading
            routingKeys.Add(RoutingKey.Create(nameof(OnOpenForTrading), Messaging.RabbitMQ.Action.NOTIFY, Event.WILDCARD));

            //Listen for successful Service execution Notification from OnOpenForTrading
            routingKeys.Add(RoutingKey.Create(nameof(OnCloseForTrading), Messaging.RabbitMQ.Action.NOTIFY, Event.WILDCARD));

            //Listen for successful Service execution Notification from OnOpenForTrading
            routingKeys.Add(RoutingKey.Create(nameof(OnOpenForTrading), Messaging.RabbitMQ.Action.UPDATESTATE, Event.WILDCARD));

            return routingKeys;
        }

        public override void Reset()
        {
            //Wait until OpenForTrading notifies before becoming active
            IsActive = false;
        }
    }
}
