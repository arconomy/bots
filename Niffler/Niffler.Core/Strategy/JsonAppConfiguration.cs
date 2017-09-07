using Niffler.Rules;
using Niffler.Rules.TradingPeriods;
using System;
using System.Collections.Generic;

namespace Niffler.Core.Strategy
{

    public class Config
    {
        public string Exchange { get; set; }
        public string StrategyId { get; set; }
    }

    public class RuleConfiguration
    {
        //Available Rules
        public static Dictionary<string, Type> RuleNames = new Dictionary<string, Type>()
        {
            {nameof(OnOpenForTrading),typeof(OnOpenForTrading) },
            {nameof(OnCloseForTrading),typeof(OnCloseForTrading) },
            {nameof(OnReduceRiskTime),typeof(OnReduceRiskTime) },
            {nameof(OnTerminateTime),typeof(OnTerminateTime) }
        };

            //RuleNames.Add(nameof(OnTickCaptureSpike));
            //RuleNames.Add(nameof(CloseTimeCancelPendingOrders));
            //RuleNames.Add(nameof(CloseTimeNoPositionsOpenedReset));
            //RuleNames.Add(nameof(CloseTimeNoPositionsRemainOpenReset));
            //RuleNames.Add(nameof(CloseTimeSetBotState));
            //RuleNames.Add(nameof(CloseTimeSetHardSLToLastPositionEntryWithBuffer));
            //RuleNames.Add(nameof(OnPositionClosedInProfitCaptureProfitPositionInfo));
            //RuleNames.Add(nameof(OnPositionClosedInProfitSetBreakEvenWithBufferIfActive));
            //RuleNames.Add(nameof(OnPositionClosedLastEntryPositionStopLossTriggeredCloseAll));
            //RuleNames.Add(nameof(OnPositionClosedReportTrade));
            //RuleNames.Add(nameof(OnPositionOpenedCaptureLastPositionInfo));
            //RuleNames.Add(nameof(OnTickBreakEvenSLActiveSetLastProfitPositionEntry));
            //RuleNames.Add(nameof(OnTickChaseFixedTrailingSL));
            //RuleNames.Add(nameof(OpenTimeCapturePrice));
            //RuleNames.Add(nameof(OpenTimeCaptureSpike));
            //RuleNames.Add(nameof(OpenTimePlaceLimitOrders));
            //RuleNames.Add(nameof(OpenTimeSetBotState));
            //RuleNames.Add(nameof(OpenTimeUseBollingerBand));
            //RuleNames.Add(nameof(ReduceRiskTimeReduceRetraceLevels));
            //RuleNames.Add(nameof(ReduceRiskTimeSetBotState));
            //RuleNames.Add(nameof(ReduceRiskTimeSetHardSLToLastProfitPositionCloseWithBuffer));
            //RuleNames.Add(nameof(ReduceRiskTimeSetTrailingStop));
            //RuleNames.Add(nameof(RetracedLevel3PlusReduceHardSLBuffer));
            //RuleNames.Add(nameof(RetracedLevel1To2SetBreakEvenSLActive));
            //RuleNames.Add(nameof(RetracedLevel1To2SetHardSLToLastProfitPositionEntryWithBuffer));
            //RuleNames.Add(nameof(RetracedLevel2To3SetFixedTrailingStop));
            //RuleNames.Add(nameof(RetracedLevel2To3SetHardSLToLastProfitPositionEntry));
            //RuleNames.Add(nameof(RetracedLevel3PlusReduceHardSLBuffer));
            //RuleNames.Add(nameof(RetracedLevel3PlusSetHardSLToLastProfitPositionCloseWithBuffer));
            //RuleNames.Add(nameof(StartTrading));
            //RuleNames.Add(nameof(EndTrading));
            //RuleNames.Add(nameof(TerminateTimeCloseAllPositionsReset));
            //RuleNames.Add(nameof(TerminateTimeSetBotState));


        public static readonly string OPENTIME = "OpenTime";
        public static readonly string OPENWEEKDAYS = "OpenWeekDays";
        public static readonly string OPENDATES = "OpenDates";
        public static readonly string CLOSETIME = "CloseTime";
        public static readonly string CLOSEAFTEROPEN = "CloseAfterOpen";
        public static readonly string REDUCERISKTIME = "ReduceRiskTime";
        public static readonly string REDUCERISKAFTEROPEN = "ReduceRiskAfterOpen";
        public static readonly string TERMINATETIME = "TerminateTime";
        public static readonly string TERMINATEAFTEROPEN = "TerminateAfterOpen";
        public static readonly string MINSPIKEPIPS = "MinSpikePips";

        public IDictionary<string,object> Params { get; set; }
        public string Name { get; set; }
    }

    public class StrategyConfiguration
    {
        public static readonly string EXCHANGE = "Exchange";
        public static readonly string STRATEGYID = "StrategyId";
        public static readonly string QUEUENAME = "QueueName";

        public string Name { get; set; }
        public Config Config { get; set; }
        public List<RuleConfiguration> Rules { get; set; }
    }

    public class JsonAppConfig
    {
        public List<StrategyConfiguration> StrategyConfig { get; set; }
    }
}
