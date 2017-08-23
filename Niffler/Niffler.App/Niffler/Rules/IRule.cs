using cAlgo.API;
using Niffler.Common;
using System;
using Niffler.Common.Trade;
using Niffler.Common.Market;
using Niffler.Common.TrailingStop;
using Niffler.Common.BackTest;
using Google.Protobuf.Collections;
using Google.Cloud.PubSub.V1;
using Niffler.Messaging;
using Niffler.Microservices;
using Niffler.Strategy;
using Niffler.Messaging.RabbitMQ;
using System.Collections.Generic;
using Niffler.Messaging.Protobuf;

namespace Niffler.Rules
{
    abstract public class IRule : IConsumer
    {
        //protected Common.State BotState;
        //protected Robot Bot;
        //protected ServicesManager RulesManager;
        //protected PositionsManager PositionsManager;
        //protected SellLimitOrdersTrader SellLimitOrdersTrader;
        //protected BuyLimitOrdersTrader BuyLimitOrdersTrader;
        //protected SpikeManager SpikeManager;
        //protected StopLossManager StopLossManager;
        //protected FixedTrailingStop FixedTrailingStop;
        //protected TradingTimeInfo MarketInfo;
        //protected Reporter Reporter;
        //protected SimplePublisher SimplePublisher;
        //protected GooglePubSubBroker MessageBroker;
        //public int Priority { get; set; }

        //  {
        //RulesManager = rulesManager;
        //BotState = rulesManager.StateManager;
        //Bot = BotState.Bot;
        //MarketInfo = BotState.GetMarketInfo();
        //PositionsManager = rulesManager.PositionsManager;
        //SellLimitOrdersTrader = rulesManager.SellLimitOrdersTrader;
        //BuyLimitOrdersTrader = rulesManager.BuyLimitOrdersTrader;
        //SpikeManager = rulesManager.SpikeManager;
        //StopLossManager = rulesManager.StopLossManager;
        //FixedTrailingStop = rulesManager.FixedTrailingStop;
        //Reporter = BotState.GetReporter();

        protected bool IsActive;
        protected RuleConfig RuleConfig;
        protected Messaging.RabbitMQ.Publisher Publisher;

        public IRule(IDictionary<string, string> botConfig, RuleConfig ruleConfig) : base(botConfig)
        {
            this.RuleConfig = ruleConfig;
            Publisher = new Messaging.RabbitMQ.Publisher(Connection,ExchangeName);
            IsActive = Init();
        }

        public override void MessageReceived(MessageReceivedEventArgs e)
        {
            switch(e.Message.Type)
            {
                case Niffle.Types.Type.Updateservice:
                    ManageRule(e.Message);
                    break;
                default:
                    {
                    if (!IsActive) return;
                    PublishResult(ExcuteRuleLogic());
                    break;
                    }
            };
        }

        //Only publishing Success or Fail - may need to look into more granualar reporting
        protected void PublishResult(bool LogicExecutionSuccess)
        {
            Service results = new Service
            {
                Action = Service.Types.Action.Notify,
                Success = LogicExecutionSuccess
            };

            RoutingKey routingKey = new RoutingKey(GetServiceName());
            Publisher.ServiceNotify(results, routingKey);
        }

        protected void ManageRule(Niffle message)
        {
            switch(message.Service.Action)
            {
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
                        ResetRule();
                        break;
                    }
            }
        }

        public void ResetRule()
        {
            Init();
            Reset();
        }
        
        abstract protected string GetServiceName();
        abstract protected bool ExcuteRuleLogic();
        abstract protected void Reset();
    }
}
