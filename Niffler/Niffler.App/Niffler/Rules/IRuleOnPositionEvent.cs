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

        override protected bool Execute() { /* IRule implementation not required */ return false;}
        abstract protected void Execute(Position position);
    }
}
