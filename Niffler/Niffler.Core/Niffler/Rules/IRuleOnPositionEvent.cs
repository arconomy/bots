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

        public void run(Position position)
        {
            if (!Initialised)
                return;

            if (!ExecuteOnce)
            {
                ExecutionCount++;
                execute(position);
            }
        }

        override protected void Execute() { /* IRule implementation not required */}
        abstract protected void execute(Position position);
    }
}
