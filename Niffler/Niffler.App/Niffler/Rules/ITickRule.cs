using cAlgo.API;
using Niffler.Messaging.Protobuf.Message;
using Niffler.Strategy;
using System.Collections.Generic;

namespace Niffler.Rules
{
    abstract class ITickRule : IRule
    {
        public ITickRule(IDictionary<string, string> botConfig, RuleConfig ruleConfig) : base(botConfig, ruleConfig) { }

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

            Tick tick = Tick.Parser.ParseFrom(e.);

            var message = 


                john = Person.Parser.ParseFrom(input);
            }


            //If message says deactivate
            Shutdown();


            //Execute logic and publish back to msg bus
            if (!Initialised)
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
