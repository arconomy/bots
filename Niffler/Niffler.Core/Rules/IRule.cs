using Niffler.Messaging.RabbitMQ;
using Niffler.Messaging.Protobuf;
using Niffler.Common;
using System;
using Niffler.Core.Config;
using Niffler.Core.Services;
using Niffler.Model;

namespace Niffler.Rules
{
    abstract public class IRule : IScalableConsumerService
    {
        protected string StrategyId;
        protected StrategyConfiguration StrategyConfig;
        protected RuleConfiguration RuleConfig;
        protected StateManager StateManager;

        protected bool IsActive = true; //Default state is active

        public IRule(StrategyConfiguration strategyConfig, RuleConfiguration ruleConfig)
        {
            this.StrategyConfig = strategyConfig;
            this.RuleConfig = ruleConfig;

            StrategyId = StrategyConfig.StrategyId;
            if (String.IsNullOrEmpty(StrategyId)) IsInitialised = false;

            ExchangeName = StrategyConfig.Exchange;
            if (String.IsNullOrEmpty(ExchangeName)) IsInitialised = false;

            //Add Rule configuration to Firebase
            StateManager = new StateManager(StrategyConfiguration.PATH, StrategyId);
            if (StateManager == null) IsInitialised = false;
            StateManager.UpdateState(RuleConfig.Params);
            StateManager.StateUpdateReceived += OnStateEventUpdate;
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
                            PublishResult(ExcuteRuleLogic(e.Message));
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

        private void OnStateEventUpdate(object sender, StateReceivedEventArgs stateupdate)
        {
            OnStateUpdate(stateupdate);
        }

        public override abstract void Init();
        abstract protected string GetServiceName();
        abstract protected bool ExcuteRuleLogic(Niffle message);
        abstract protected void OnServiceNotify(Niffle message, RoutingKey routingKey);
        abstract protected void OnStateUpdate(StateReceivedEventArgs stateupdate);
    }
}
