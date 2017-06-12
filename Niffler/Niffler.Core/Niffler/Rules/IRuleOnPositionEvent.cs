using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo.API;

namespace Niffler.Rules
{
    abstract class IRuleOnPositionEvent : IRule
    {
        public IRuleOnPositionEvent(int priority) : base(priority) { }

        public void Run(Position position)
        {
            if (!Initialised)
                return;

            if (!ExecuteOnce)
            {
                ExecutionCount++;
                Execute(position);
                RunExecutionLogging();
            }
        }

        override protected void Execute() { /* IRule implementation not required */}
        abstract protected void Execute(Position position);
    }
}
