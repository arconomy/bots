using Niffler.Strategy;
using Niffler.Messaging.RabbitMQ;
using System.Collections.Generic;
using Niffler.Messaging.Protobuf;

namespace Niffler.Rules
{
    abstract public class IRule : IConsumer
    {
        protected bool IsInitialised;
        protected bool IsActive = true; //Default state is active
        protected RuleConfig RuleConfig;
        protected Messaging.RabbitMQ.Publisher Publisher;

        public IRule(IDictionary<string, string> botConfig, RuleConfig ruleConfig) : base(botConfig)
        {
            this.RuleConfig = ruleConfig;
            Publisher = new Messaging.RabbitMQ.Publisher(Connection,ExchangeName);
            IsInitialised = Init();
        }

        public override void MessageReceived(MessageReceivedEventArgs e)
        {
            switch(e.Message.Type)
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
            Service results = new Service
            {
                Action = Service.Types.Action.Notify,
                Success = LogicExecutionSuccess
            };

            Publisher.ServiceNotify(results, GetServiceName());
        }

        protected void ManageRule(Niffle message, RoutingKey routingKey)
        {
            switch(message.Service.Action)
            {
                case Service.Types.Action.Notify:
                    {
                        OnServiceNotify(message, routingKey);
                        break;
                    }
                case Service.Types.Action.Activate:
                    {
                        IsActive = true;
                        break;
                    }
                case Service.Types.Action.Deactivate:
                    {
                        IsActive = false;
                        break;
                    }
                case Service.Types.Action.Scaleup:
                    {
                        InitAutoScale(QueueName);
                        break;
                    }
                case Service.Types.Action.Scaledown:
                    {
                        break;
                    }
                case Service.Types.Action.Shutdown:
                    {
                        Shutdown();
                        break;
                    }
                case Service.Types.Action.Reset:
                    {
                        Reset();
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

        public void Reset()
        {
            IsInitialised = Init();
        }
        
        abstract protected string GetServiceName();
        abstract protected bool ExcuteRuleLogic(Niffle message);
        abstract protected void OnServiceNotify(Niffle message, RoutingKey routingKey);
    }
}
