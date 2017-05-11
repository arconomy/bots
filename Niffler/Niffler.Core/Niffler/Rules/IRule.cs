using cAlgo.API;
using Niffler.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niffler.Rules
{
    interface IRule
    {
        bool execute(Robot Bot, State BotState);
        void reportExecution();
    }
}
