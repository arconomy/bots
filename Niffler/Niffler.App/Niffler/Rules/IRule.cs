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

namespace Niffler.Rules
{
    abstract class IRule
    {
        protected StateManager BotState;
        protected Robot Bot;
        protected ServicesManager RulesManager;
        protected PositionsManager PositionsManager;
        protected SellLimitOrdersTrader SellLimitOrdersTrader;
        protected BuyLimitOrdersTrader BuyLimitOrdersTrader;
        protected SpikeManager SpikeManager;
        protected StopLossManager StopLossManager;
        protected FixedTrailingStop FixedTrailingStop;
        protected MarketTradeTimeInfo MarketInfo;
        protected Reporter Reporter;
        protected SimplePublisher SimplePublisher;
        protected GooglePubSubBroker MessageBroker;
        public int Priority { get; set; }
        protected int ExecutionCount;
        protected bool ExecuteOnce;
        protected bool LogEveryExecution;
        protected bool Initialised;


        public IRule(int priority)
        {
            Priority = priority;
        }

        public void Init(ServicesManager rulesManager)
        {
            RulesManager = rulesManager;
            BotState = rulesManager.StateManager;
            Bot = BotState.Bot;
            MarketInfo = BotState.GetMarketInfo();
            PositionsManager = rulesManager.PositionsManager;
            SellLimitOrdersTrader = rulesManager.SellLimitOrdersTrader;
            BuyLimitOrdersTrader = rulesManager.BuyLimitOrdersTrader;
            SpikeManager = rulesManager.SpikeManager;
            StopLossManager = rulesManager.StopLossManager;
            FixedTrailingStop = rulesManager.FixedTrailingStop;
            Reporter = BotState.GetReporter();
            MessageBroker = rulesManager.MessageBroker;
            CreateSimplePublisher(GetPubSubTopicName());
            Initialised = true;
        }
        
        public async void CreateSimplePublisher(String topicId)
        {
            TopicName topicName = MessageBroker.CreatePubSubTopic(topicId);
            SimplePublisher = await SimplePublisher.CreateAsync(topicName);
        }

        public void Run()
        {
            if (!Initialised)
                return;

            if(IsTradingRule())
            {
                if (!BotState.IsTrading)
                    return;
            }

            if(!ExecuteOnce)
            {
                ExecutionCount++;
                PublishExecutionResult(Execute());
                RunExecutionLogging();
            }
        }

        protected void RunExecutionLogging()
        {
            if (LogEveryExecution)
                LogExecution();
        }

        public void ResetRule()
        {
            ExecuteOnce = false;
            ExecutionCount = 0;
            Reset();
        }

        //Flag that can be set by implemented Rule to ensure rule is only executed once
        protected void ExecuteOnceOnly()
        {
            ExecuteOnce = true;
        }
        
        public void ReportExecutionCount()
        {
            Reporter.ReportRuleExecutionCount(this,ExecutionCount);
        }
        
        //Use to log Rules everytime Rule executes in order to see which rules executed prior to a trade closing
        protected void LogExecutions()
        {
            LogEveryExecution = true;
        }

        private void LogExecution()
        {
            Reporter.LogRuleExecution(this);
        }

        //Publish rule action if completed successfully
        protected async void PublishExecutionResult(bool ruleActionCompleted)
        {
            //Only publish if rule action was completed
            if(ruleActionCompleted)
            {
                await SimplePublisher.PublishAsync(MessageBroker.GetPubSubMessage(ruleActionCompleted, GetLastExecutionData()));
            }
        }
        
        abstract protected bool IsTradingRule();
        abstract protected bool Execute();
        abstract protected void Reset();
        abstract public MapField<String, String> GetLastExecutionData();
        abstract public string GetPubSubTopicName();
    }
}
