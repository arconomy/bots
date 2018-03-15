using Niffler.Rules;
using Niffler.Rules.TradingPeriods;
using System;
using System.Collections.Generic;

namespace Niffler.Core.Config
{

    public class JsonAppConfig
    {
        public List<StrategyConfiguration> StrategyConfig { get; set; }
    }

    public class StrategyConfiguration
    {
        public static readonly string EXCHANGE = "Exchange";
        public static readonly string STRATEGYID = "StrategyId";
        public static readonly string BROKERID = "BrokerId";
        public static readonly string QUEUENAME = "QueueName";
        public static readonly string BASEURL = "https://niffler-176904.firebaseio.com/";
        public static readonly string STATEPATH = "state/";
        public static readonly string RULESPATH = "rules/";

        public string Name { get; set; }
        public string Exchange { get; set; }
        public string StrategyId { get; set; }
        public string BrokerId { get; set; }
        public List<RuleConfiguration> Rules { get; set; }
    }
}
