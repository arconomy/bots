using Niffler.Messaging.RabbitMQ;
using System.Collections.Generic;
using Niffler.Messaging.Protobuf;
using Niffler.Core.Config;
using static Niffler.Model.State;
using Niffler.Common;
using Niffler.Rules.TradingPeriods;
using Niffler.Model;

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

            //Listen for state updates
            StateManager.ListenForStateUpdates();
        }

        //Execute rule logic
        override protected bool ExcuteRuleLogic(Niffle message)
        {
            if (IsTickMessageEmpty(message)) return false;
            if (SpikeStartPrice == 0) return false; //If the Spike Start price had not been set then exit

            bool notifyOfSpikePeakCapture = false;
            if(SetSpikeUpPeak(Utils.GetMidPrice(message.Tick)))
            {
                StateManager.SetInitialStateAsync(new State
                    {
                        { State.SPIKEUPPEAK, SpikeUpPeakPips }
                    });
                notifyOfSpikePeakCapture = true;
            }

            if (SetSpikeDownPeak(Utils.GetMidPrice(message.Tick)))
            {
                StateManager.SetInitialStateAsync(new State
                    {
                        { State.SPIKEDOWNPEAK, SpikeDownPeakPips }
                    });
                notifyOfSpikePeakCapture = true;
            }

            if(SetSpikeDirection())
            {
                StateManager.SetInitialStateAsync(new State
                    {
                        { State.SPIKEDIRECTION, SpikeDirection }
                    });
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
            //Nothing Required
        }

        protected override void OnStateUpdate(StateChangedEventArgs stateupdate)
        {
            //Listening for update to OpenPrice State
            if (stateupdate.Key == Model.State.OPENPRICE)
            {
                if (double.TryParse(stateupdate.Value.ToString(), out double openPrice))
                    SetSpikeStart(openPrice);
            }
        }

        protected override string GetServiceName()
        {
            return nameof(CaptureSpike);
        }

        protected override void AddListeningRoutingKeys(ref List<RoutingKey> routingKeys)
        {
            //Listen for OnTick
            routingKeys.Add(RoutingKey.Create(Source.WILDCARD, Messaging.RabbitMQ.Action.WILDCARD, Event.ONTICK));
        }

        public override void Reset()
        {
            //Wait until OpenForTrading notifies before becoming active
            SetActiveState(false);
        }
    }
}
