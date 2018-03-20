using Niffler.Messaging.RabbitMQ;
using Niffler.Messaging.Protobuf;
using Niffler.Common;
using System;
using Niffler.Core.Config;
using Niffler.Core.Services;
using Niffler.Core.Trades;
using Niffler.Model;
using System.Collections.Generic;
using System.Collections;

namespace Niffler.Rules
{
    abstract public class IRule : IScalableConsumerService
    {
        protected string StrategyId;
        protected StrategyConfiguration StrategyConfig;
        protected string BrokerId;
        protected RuleConfiguration RuleConfig;
        protected TradeUtils TradeUtils;
        protected StateManager StateManager;
        protected TradesFactory TradesFactory;
        protected TradePublisher TradePublisher;

        protected Dictionary<string, bool> ActivateRules = new Dictionary<string, bool>();
        protected Dictionary<string, bool> DeactivateRules = new Dictionary<string, bool>();

        private bool IsActive = true; //Default state is active

        public IRule(StrategyConfiguration strategyConfig, RuleConfiguration ruleConfig)
        {
            this.StrategyConfig = strategyConfig;
            this.RuleConfig = ruleConfig;

            StrategyId = StrategyConfig.StrategyId;
            if (String.IsNullOrEmpty(StrategyId)) IsInitialised = false;

            ExchangeName = StrategyConfig.Exchange;
            if (String.IsNullOrEmpty(ExchangeName)) IsInitialised = false;

            BrokerId = StrategyConfig.BrokerId;
            if (String.IsNullOrEmpty(BrokerId)) IsInitialised = false;

            //Initialise TradeUtils for broker specific config
            TradeUtils = new TradeUtils(new BrokerConfiguration(BrokerId));

            //Initialise a TradesPublisher
            TradePublisher = new TradePublisher(Publisher, TradeUtils, GetServiceName());

            //Add Rule configuration to Firebase
            StateManager = new StateManager(StrategyId);
            if (StateManager == null) IsInitialised = false;
            StateManager.SetInitialState(RuleConfig.Params);

            //Manage State updates if subscribed to by the derived rule
            StateManager.StateUpdateReceived += OnStateEventUpdate;

            StateManager.SetActivationRules(GetServiceName(), RuleConfig.ActivateRules);
            StateManager.SetDeactivationRules(GetServiceName(), RuleConfig.DeactivateRules);

            //If this rule has activation rules default state IsActive = false
            if(RuleConfig.ActivateRules != null)
            {
                IsActive = false;
            }
            StateManager.UpdateRuleStatus(GetServiceName(), RuleConfiguration.ISACTIVE, IsActive);
        }

        protected void SetActiveState(bool isActive)
        {
            StateManager.UpdateRuleStatus(GetServiceName(), RuleConfiguration.ISACTIVE,isActive);
            IsActive = isActive;
        }

        private void OnStateEventUpdate(object sender, StateChangedEventArgs stateupdate)
        {
            OnStateUpdate(stateupdate);
        }

        protected override void OnMessageReceived(Object o, MessageReceivedEventArgs e)
        {
            //Check if msg is Strategy specific
            if (e.Message.IsStrategyIdRequired)
            {
                if (e.Message.StrategyId != StrategyId) return;
            }

            switch (e.Message.Type)
            {
                case Niffle.Types.Type.Service:
                    OnServiceMessageReceived(e.Message, new RoutingKey(e.EventArgs.RoutingKey));
                    break;
                default:
                    {
                    if (IsInitialised && IsActive)
                        {
                            PublishResult(ExcuteRuleLogic(e.Message));
                        }
                            
                    break;
                    }
            };
        }

        protected void OnServiceMessageReceived(Niffle message, RoutingKey routingKey)
        {
            switch (message.Service.Command)
            {
                case Service.Types.Command.Notify:
                    {
                        //Test for Service Message
                        if (IsServiceMessageEmpty(message))
                        {
                            Console.WriteLine("ERROR: Service Notify Type message received with no Service message");
                            return;
                        }

                        OnServiceActivationNotify(message, routingKey);
                        OnServiceNotify(message, routingKey);
                        break;
                    }
                case Service.Types.Command.Reset:
                    {
                        Reset();
                        break;
                    }
                case Service.Types.Command.Shutdown:
                    {
                        ShutDown();
                        break;
                    }
            }
        }

        //Only publishing Success or Fail - look at more granualar reporting action taken
        protected void PublishResult(bool LogicExecutionSuccess)
        {
            string timestamp = Utils.FormatDateTimeWithSeparators(System.DateTime.Now);

            Service results = new Service
            {
                Command = Service.Types.Command.Notify,
                Success = LogicExecutionSuccess
            };

            Publisher.ServiceNotify(results, GetServiceName(), StrategyId);
        }

        protected bool IsTickMessageEmpty(Niffle message)
        {
            if (message.Type != Niffle.Types.Type.Tick) return true;
            if (message.Tick == null) return true;
            return false;
        }

        protected bool IsOnPositionMessageEmpty(Niffle message)
        {
            if (message.Type != Niffle.Types.Type.Position) return true;
            if (message.Position == null) return true;
            return false;
        }

        protected bool IsServiceMessageEmpty(Niffle message)
        {
            if (message.Type != Niffle.Types.Type.Service) return true;
            if (message.Service == null) return true;
            return false;
        }

        public override void ShutDown()
        {
            ShutDownService();
        }

        private void OnServiceActivationNotify(Niffle message, RoutingKey routingKey)
        {
            //Listening for activation notifications
            if(ActivateRules.ContainsKey(routingKey.Source) && message.Service.Success)
            {
                ActivateRules[routingKey.Source] = true;
            }

            if (!ActivateRules.ContainsValue(false) && !IsActive)
            {
                SetActiveState(true);
            }

            //Listening for deactivation notifications
            if (DeactivateRules.ContainsKey(routingKey.Source) && message.Service.Success)
            {
                DeactivateRules[routingKey.Source] = true;
            }

            if (!DeactivateRules.ContainsValue(false) && IsActive)
            {
                SetActiveState(false);
            }
        }

        //Listen for a success notification from activation and deactivation rules
        private void AddRoutingKeys(ref List<RoutingKey> routingKeys)
        {
            if (RuleConfig.ActivateRules != null)
            {
                foreach (string ruleName in RuleConfig.ActivateRules)
                {
                    ActivateRules.Add(ruleName, false);
                    routingKeys.Add(RoutingKey.Create(ruleName, Messaging.RabbitMQ.Action.NOTIFY, Event.WILDCARD));
                }
            }

            if (RuleConfig.DeactivateRules != null)
            {
                foreach (string ruleName in RuleConfig.DeactivateRules)
                {
                    DeactivateRules.Add(ruleName, false);
                    routingKeys.Add(RoutingKey.Create(ruleName, Messaging.RabbitMQ.Action.NOTIFY, Event.WILDCARD));
                }
            }
        }
        
        protected override List<RoutingKey> SetListeningRoutingKeys()
        {
            List<RoutingKey> routingKeys = new List<RoutingKey>();
            AddRoutingKeys(ref routingKeys);
            AddListeningRoutingKeys(ref routingKeys);
            return routingKeys;
        }

        protected String GetSymbolCode()
        {
            String symbolCode = StrategyConfig.Exchange;
            if (String.IsNullOrEmpty(symbolCode)) IsInitialised = false;
            return symbolCode;
        }

        protected int GetRuleConfigIntegerParam(String ruleConfigParamName)
        {
            int paramInt = -1;
            if (RuleConfig.Params.TryGetValue(ruleConfigParamName, out object paramIntObj))
            {
                if (!Double.TryParse(paramIntObj.ToString(), out paramInt))
                {
                    IsInitialised = false;
                }
            }
            return paramInt;
        }


        protected double GetRuleConfigDoubleParam(String ruleConfigParamName)
        {
            double paramDouble = -1;
            if (RuleConfig.Params.TryGetValue(ruleConfigParamName, out object paramDoubleObj))
            {
                if (!Double.TryParse(paramDoubleObj.ToString(), out paramDouble))
                {
                    IsInitialised = false;
                }
            }
            return paramDouble;
        }

        protected bool GetRuleConfigBoolParam(String ruleConfigParamName)
        {
            bool paramBool = false;
            if (RuleConfig.Params.TryGetValue(ruleConfigParamName, out object paramBoolObj))
            {
                if (!Boolean.TryParse(paramBoolObj.ToString(), out paramBool))
                {
                    IsInitialised = false;
                }
            }
            return paramBool;
        }






        public override abstract void Init();
        abstract protected string GetServiceName();
        abstract protected bool ExcuteRuleLogic(Niffle message);
        abstract protected void OnServiceNotify(Niffle message, RoutingKey routingKey);
        abstract protected void OnStateUpdate(StateChangedEventArgs stateupdate);
        protected abstract void AddListeningRoutingKeys(ref List<RoutingKey> routingKeys);

    }
}
