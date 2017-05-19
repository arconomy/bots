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

namespace Niffler.Rules
{
    abstract class IRule
    {
        protected State BotState;
        protected Robot Bot;
        protected RulesManager RulesManager;
        protected PositionsManager PositionsManager;
        protected OrdersManager OrdersManager;
        protected SpikeManager SpikeManager;
        protected StopLossManager StopLossManager;
        protected FixedTrailingStop FixedTrailingStop;
        public int Priority { get; set; }
        protected int ExecutionCount;
        protected bool ExecuteOnce;

        public IRule(RulesManager rulesManager, int priority)
        {
            BotState = rulesManager.BotState;
            Bot = BotState.Bot;
            PositionsManager = rulesManager.PositionsManager;
            OrdersManager = rulesManager.OrdersManager;
            SpikeManager = rulesManager.SpikeManager;
            StopLossManager = rulesManager.StopLossManager;
            FixedTrailingStop = rulesManager.FixedTrailingStop;
            Priority = priority;
        }

        public void run()
        {
            if(!ExecuteOnce)
            {
                ExecutionCount++;
                execute();
            }
        }

        protected void ExecuteOnceOnly()
        {
            ExecuteOnce = true;
        }

        abstract public void reportExecution();
        abstract protected void execute();
    }
}
