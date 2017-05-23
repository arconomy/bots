using cAlgo.API;
using Niffler.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Niffler.Common.Trade;
using Niffler.Common.Market;
using Niffler.Common.TrailingStop;
using Niffler.Common.BackTest;

namespace Niffler.Rules
{
    abstract class IRule
    {
        protected State BotState;
        protected Robot Bot;
        protected RulesManager RulesManager;
        protected PositionsManager PositionsManager;
        protected SellLimitOrdersTrader SellLimitOrdersTrader;
        protected BuyLimitOrdersTrader BuyLimitOrdersTrader;
        protected SpikeManager SpikeManager;
        protected StopLossManager StopLossManager;
        protected FixedTrailingStop FixedTrailingStop;
        protected MarketInfo MarketInfo;
        protected Reporter Reporter;
        public int Priority { get; set; }
        protected int ExecutionCount;
        protected bool ExecuteOnce;
        protected bool Initialised;

        public IRule(int priority)
        {
            Priority = priority;
        }

        public void Init(RulesManager rulesManager)
        {
            RulesManager = rulesManager;
            BotState = rulesManager.BotState;
            Bot = BotState.Bot;
            MarketInfo = BotState.GetMarketInfo();
            PositionsManager = rulesManager.PositionsManager;
            SellLimitOrdersTrader = rulesManager.SellLimitOrdersTrader;
            BuyLimitOrdersTrader = rulesManager.BuyLimitOrdersTrader;
            SpikeManager = rulesManager.SpikeManager;
            StopLossManager = rulesManager.StopLossManager;
            FixedTrailingStop = rulesManager.FixedTrailingStop;
            Reporter = BotState.GetReporter();
            Initialised = true;
        }

        public void Run()
        {
            if (!Initialised)
                return;

            if(!ExecuteOnce)
            {
                ExecutionCount++;
                Execute();
            }
        }

        protected void ExecuteOnceOnly()
        {
            ExecuteOnce = true;
        }

        abstract public void ReportExecution();
        abstract protected void Execute();
    }
}
