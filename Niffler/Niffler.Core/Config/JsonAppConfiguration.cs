using static Niffler.Core.Trades.TradeVolumeCalculator;
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
        public static readonly string RULESPATH = "/rules/";
        public static readonly string TRADESPATH = "trades/";

        public string Name { get; set; }
        public string Exchange { get; set; }
        public string StrategyId { get; set; }
        public string BrokerId { get; set; }
        public List<RuleConfiguration> Rules { get; set; }
    }

    public class VolumeConfiguration
    {
        public bool EnableDynamicVolumeIncrease { get; set; }
        public double VolumeBase { get; set; }
        public double VolumeMax { get; set; }
        public CalculationType Type { get; set; }
        public double VolumeIncrement { get; set; }
        public double VolumeMultiplier { get; set; }
        public double VolumeIncreaseFactor { get; set; }
        public int IncreaseVolumeAfterOrders { get; set; }
    }

    public class OrderSpacingConfiguration
    {
        public bool EnableDynamicOrderSpacing { get; set; }
        public double OrderSpacingBasePips { get; set; }
        public double OrderSpacingMaxPips { get; set; }
        public double OrderSpacingIncrementPips { get; set; }
        public int IncrementSpacingAfterOrders { get; set; }
    }
}
