using Niffler.Strategy;
using Niffler.Messaging.RabbitMQ;
using System.Collections.Generic;
using Niffler.Messaging.Protobuf;
using Niffler.Common;

namespace Niffler.Rules
{
    abstract public class IRule : Consumer
    {
        protected string StrategyId;
        protected bool IsInitialised;
        protected bool IsActive = true; //Default state is active
        protected RuleConfiguration RuleConfig;
        protected Messaging.RabbitMQ.Publisher Publisher;

        public IRule(StrategyConfiguration strategyConfig, RuleConfiguration ruleConfig) : base(strategyConfig)
        {
            StrategyConfig.Config.TryGetValue(StrategyConfiguration.STRATEGYID, out StrategyId);
            this.RuleConfig = ruleConfig;
            Publisher = new Messaging.RabbitMQ.Publisher(Connection,ExchangeName);

            IsInitialised = Init();
        }

        public override void MessageReceived(MessageReceivedEventArgs e)
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
        protected void PublishStateUpdate(string StrategyId, string key, bool value)
        {
            State stateUpdate = new State()
            {
                Valuetype = State.Types.ValueType.Bool,
                BoolValue = value
            };

            Publisher.UpdateState(stateUpdate, GetServiceName(), StrategyId);
        }

        //Publish State update message
        protected void PublishStateUpdate(string StrategyId,string key, string value)
        {
            State stateUpdate = new State()
            {
                Valuetype = State.Types.ValueType.String,
                StringValue = value
            };

            Publisher.UpdateState(stateUpdate, GetServiceName(), StrategyId);
        }

        //Publish State update message
        protected void PublishStateUpdate(string StrategyId, string key, double value)
        {
            State stateUpdate = new State()
            {
                Valuetype = State.Types.ValueType.Double,
                DoubleValue = value
            };

            Publisher.UpdateState(stateUpdate, GetServiceName(), StrategyId);
        }


        protected void ManageRule(Niffle message, RoutingKey routingKey)
        {
            switch(message.Service.Command)
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
                        Shutdown();
                        break;
                    }
            }
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

        protected bool IsOnPositionsMessageEmpty(Niffle message)
        {
            if (message.Type != Niffle.Types.Type.Positions) return true;
            if (message.Positions == null) return true;
            return false;
        }

        protected bool IsServiceMessageEmpty(Niffle message)
        {
            if (message.Type != Niffle.Types.Type.Positions) return true;
            if (message.Positions == null) return true;
            return false;
        }

        protected bool IsStateMessageEmpty(Niffle message)
        {
            if (message.Type != Niffle.Types.Type.State) return true;
            if (message.State == null) return true;
            return false;
        }

        protected override List<RoutingKey> GetListeningRoutingKeys()
        {
            //Set Rule specific routingKeys
            List<RoutingKey> routingKeys = SetListeningRoutingKeys();

            //Listen for any SHUTDOWN Action message on the exchange
            routingKeys.Add(RoutingKey.Create(Messaging.RabbitMQ.Action.SHUTDOWN));

            //Listen for any RESET Action message on the exchange
            routingKeys.Add(RoutingKey.Create(Messaging.RabbitMQ.Action.RESET));

            return routingKeys;
        }

        public void Reset()
        {
            IsInitialised = Init();
        }

        public override object Clone(StrategyConfiguration strategyConfig,
                       int timeout = 10, ushort prefetchCount = 1, bool autoAck = true,
                                   IDictionary<string, object> queueArgs = null)
        {
            return Clone(strategyConfig, RuleConfig);
        }

        abstract public object Clone(StrategyConfiguration strategyConfig, RuleConfiguration ruleConfig);
        abstract protected string GetServiceName();
        abstract protected bool ExcuteRuleLogic(Niffle message);
        abstract protected List<RoutingKey> SetListeningRoutingKeys();
        abstract protected void OnServiceNotify(Niffle message, RoutingKey routingKey);
        abstract protected void OnStateUpdate(Niffle message, RoutingKey routingKey);
    }
}
