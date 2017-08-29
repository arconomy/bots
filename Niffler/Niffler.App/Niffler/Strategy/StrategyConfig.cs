using Niffler.Rules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niffler.Strategy
{

    public class AppConfiguration
    {
        public List<StrategyConfiguration> StrategyConfigList = new List<StrategyConfiguration>();
    }

    public class StrategyConfiguration
    {
        public static readonly string MARKET = "Market";
        public static readonly string STRATEGYID = "StrategyId";
        public string Name { get; set; }
        public IDictionary<string,string> Config = new Dictionary<string,string>();
        public List<RuleConfiguration> Rules = new List<RuleConfiguration>();
    }

    public class RuleConfiguration
    {
        public static readonly string OPENTIME = "OpenTime";
        public static readonly string OPENWEEKDAYS = "OpenWeekDays";
        public static readonly string OPENDATES = "OpenDates";
        public static readonly string CLOSETIME = "OpenDates";
        public static readonly string CLOSEAFTEROPEN = "CloseAfterOpen";
        public static readonly string MINSPIKEPIPS = "MinSpikePips";
        
        public string Name { get; set; }
        public IDictionary<string, object> Params = new Dictionary<string, object>();

    }
}
