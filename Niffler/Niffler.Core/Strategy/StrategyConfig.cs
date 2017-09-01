using System.Collections.Generic;

namespace Niffler.Core.Strategy
{

    public class AppConfiguration
    {
        public List<StrategyConfiguration> StrategyConfigList = new List<StrategyConfiguration>();
    }

    public class StrategyConfiguration
    {
        public static readonly string EXCHANGE = "Exchange";
        public static readonly string STRATEGYID = "StrategyId";
        public static readonly string QUEUENAME = "QueueName";
        public string Name { get; set; }
        public IDictionary<string,string> Config = new Dictionary<string,string>();
        public List<RuleConfiguration> Rules = new List<RuleConfiguration>();
    }

    public class RuleConfiguration
    {
        public static readonly string OPENTIME = "OpenTime";
        public static readonly string OPENWEEKDAYS = "OpenWeekDays";
        public static readonly string OPENDATES = "OpenDates";
        public static readonly string CLOSETIME = "CloseTime";
        public static readonly string CLOSEAFTEROPEN = "CloseAfterOpen";
        public static readonly string REDUCERISKTIME = "ReduceRiskTime";
        public static readonly string REDUCERISKAFTEROPEN = "ReduceRiskAfterOpen";
        public static readonly string TERMINATETIME = "TerminateRiskTime";
        public static readonly string TERMINATEAFTEROPEN = "TerminateAfterOpen";
        public static readonly string MINSPIKEPIPS = "MinSpikePips";
        
        public string Name { get; set; }
        public IDictionary<string, object> Params = new Dictionary<string, object>();

    }
}
