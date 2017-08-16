using Niffler.Rules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niffler.Strategy
{

    public class StrategyConfig
    {
        public List<BotConfig> BotConfig = new List<BotConfig>();
    }

    public class BotConfig
    {
        public string Name { get; set; }
        public IDictionary<string,string> Config = new Dictionary<string,string>();
        public List<RuleConfig> Rules = new List<RuleConfig>();
    }

    public class RuleConfig
    {
        public string Name { get; set; }
        public IDictionary<string, string> Params = new Dictionary<string, string>();
    }
}
