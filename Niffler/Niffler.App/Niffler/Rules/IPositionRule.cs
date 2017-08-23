using cAlgo.API;
using Niffler.Strategy;
using System.Collections.Generic;

namespace Niffler.Rules
{
    abstract class IPositionRule : IRule
    {
        public IPositionRule(IDictionary<string, string> botConfig, RuleConfig ruleConfig) : base(botConfig, ruleConfig) { }

        public void Run(Position position)
        {
            if (!IsActive)
                return;

            if (!ExecuteOnce)
            {
                ExecutionCount++;
                Execute(position);
                RunExecutionLogging();
            }
        }

        public override void MessageReceived(MessageReceivedEventArgs e)
        {
            //Manage messages for positions
            //Position p = new Position { };


            //If message says deactivate
            Shutdown();


            //Execute logic and publish back to msg bus
            if (!IsActive)
                return;

            if (IsTradingRule())
            {
                if (!BotState.IsTrading)
                    return;
            }

            ExcuteRuleLogic();


            if (!ExecuteOnce)
            {
                ExecutionCount++;
                PublishExecutionResult(ExcuteRuleLogic());
                RunExecutionLogging();
            }

        }

        override protected bool ExcuteRuleLogic() { /* IRule implementation not required */ return false;}
        abstract protected void Execute(Position position);
    }
}
