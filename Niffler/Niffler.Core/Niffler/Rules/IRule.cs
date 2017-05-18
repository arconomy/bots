using cAlgo.API;
using Niffler.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Niffler.Common.Trade;
using Niffler.Common.Market;

namespace Niffler.Rules
{
    abstract class IRule
    {
        protected State BotState;
        protected Robot Bot;
        protected PositionsManager PositionsManager;
        protected OrdersManager OrdersManager;
        protected SpikeManager SpikeManager;
        protected StopLossManager StopLossManager;

        public IRule(RulesManager rulesManager)
        {
            BotState = rulesManager.BotState;
            Bot = BotState.Bot;
            PositionsManager = rulesManager.PositionsManager;
            OrdersManager = rulesManager.OrdersManager;
            SpikeManager = rulesManager.SpikeManager;
            StopLossManager = rulesManager.StopLossManager;
        }

        abstract public void execute();
        abstract public void reportExecution();
    }
}
