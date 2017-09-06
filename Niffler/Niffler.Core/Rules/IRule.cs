using Niffler.Messaging.RabbitMQ;
using Niffler.Messaging.Protobuf;
using Niffler.Common;
using System;
using Niffler.Core.Strategy;

namespace Niffler.Rules
{
    abstract public class IRule : IScalableConsumerService
    {
        protected string StrategyId;
        protected StrategyConfiguration StrategyConfig;
        protected RuleConfiguration RuleConfig;

        protected bool IsActive = true; //Default state is active

        public IRule(StrategyConfiguration strategyConfig, RuleConfiguration ruleConfig)
        {
            this.StrategyConfig = strategyConfig;
            this.RuleConfig = ruleConfig;

            if (!StrategyConfig.Config.TryGetValue(StrategyConfiguration.STRATEGYID, out StrategyId)) IsInitialised = false;
            if (!StrategyConfig.Config.TryGetValue(StrategyConfiguration.EXCHANGE, out ExchangeName)) IsInitialised = false;
        }

        protected override void OnMessageReceived(Object o, MessageReceivedEventArgs e)
        {
            //Only interested in messages for this Strategy
            if (e.Message.StrategyId != StrategyId) return;

            switch (e.Message.Type)
            {
                case Niffle.Types.Type.Service:
                    ManageRule(e.Message, new RoutingKey(e.EventArgs.RoutingKey));
                    break;
                default:
                    {
                    if (!IsInitialised && !IsActive) return;
                    PublishResult(ExcuteRuleLogic(e.Message));
                    break;
                    }
            };
        }

        protected void ManageRule(Niffle message, RoutingKey routingKey)
        {
            switch (message.Service.Command)
            {
                case Service.Types.Command.Notify:
                    {
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

        //Publish State update message
        protected void PublishStateUpdate(string key, bool value)
        {
            State stateUpdate = new State()
            {
                ValueType = State.Types.ValueType.Bool,
                BoolValue = value
            };

            Publisher.UpdateState(stateUpdate, GetServiceName(), StrategyId);
        }

        //Publish State update message
        protected void PublishStateUpdate(string key, string value)
        {
            State stateUpdate = new State()
            {
                ValueType = State.Types.ValueType.String,
                StringValue = value
            };

            Publisher.UpdateState(stateUpdate, GetServiceName(), StrategyId);
        }

        //Publish State update message
        protected void PublishStateUpdate(string key, double value)
        {
            State stateUpdate = new State()
            {
                ValueType = State.Types.ValueType.Double,
                DoubleValue = value
            };

            Publisher.UpdateState(stateUpdate, GetServiceName(), StrategyId);
        }

        protected bool IsTickMessageEmpty(Niffle message)
        {
            if (message.Type != Niffle.Types.Type.Tick) return false;
            if (message.Tick == null) return false;
            return true;
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

        protected bool IsStateMessageEmpty(Niffle message)
        {
            if (message.Type != Niffle.Types.Type.State) return true;
            if (message.State == null) return true;
            return false;
        }

        public override void ShutDown()
        {
            ShutDownService();
        }

        public override abstract void Init();
        abstract protected string GetServiceName();
        abstract protected bool ExcuteRuleLogic(Niffle message);
        abstract protected void OnServiceNotify(Niffle message, RoutingKey routingKey);
        abstract protected void OnStateUpdate(Niffle message, RoutingKey routingKey);
    }
}
