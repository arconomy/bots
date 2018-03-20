using Niffler.Rules;
using Niffler.Rules.Capture;
using Niffler.Rules.TradingPeriods;
using System;
using System.Collections.Generic;

namespace Niffler.Core.Config
{ 
    public class RuleConfiguration
    {
        //Available Rules
        public static Dictionary<string, Type> RuleNames = new Dictionary<string, Type>()
        {
            {nameof(OnOpenForTrading),typeof(OnOpenForTrading) },
            {nameof(OnCloseForTrading),typeof(OnCloseForTrading) },
            {nameof(OnReduceRiskTime),typeof(OnReduceRiskTime) },
            {nameof(OnTerminateTime),typeof(OnTerminateTime) },
            {nameof(CaptureSpike),typeof(CaptureSpike) },
            {nameof(OnOpenPlaceSellLimit),typeof(OnOpenPlaceSellLimit) }
        };

                //RuleNames.Add(nameof(OpenTimePlaceLimitOrders)); Implementing this..


        // {nameof(CaptureOpenPrice),typeof(CaptureOpenPrice)

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

        //key names for JSON values
        public const string ACTIVATERULES = "ActivateRules";
        public const string DEACTIVATERULES = "DeactivateRules";
        public const string OPENTIME = "OpenTime";
        public const string OPENWEEKDAYS = "OpenWeekDays";
        public const string OPENANYDATE = "OpenAnyDate";
        public const string OPENDATES = "OpenDates";
        public const string CLOSETIME = "CloseTime";
        public const string CLOSEAFTEROPEN = "CloseAfterOpen";
        public const string REDUCERISKTIME = "ReduceRiskTime";
        public const string REDUCERISKAFTEROPEN = "ReduceRiskAfterOpen";
        public const string TERMINATETIME = "TerminateTime";
        public const string TERMINATEAFTEROPEN = "TerminateAfterOpen";
        public const string MINSPIKEPIPS = "MinSpikePips";
        public const string PIPSTOTAL = "PipsTotal";
        public const string PROFITTOTAL = "ProfitTotal";
        public const string ORDERSPLACEDCOUNT = "OrdersPlacedCount";
        public const string ERRORCOUNT = "ErrorCount";
        public const string POSITIONSOPENEDCOUNT = "PositionsOpenedCount";
        public const string POSITIONCLOSEDCOUNT = "PositionsClosedCount";
        public const string ACTIVATIONSTATUS = "/Activate";
        public const string DEACTIVATESTATUS = "/Deactivate";
        public const string ISACTIVE = "IsActive";
        public const string EXECUTEONLYONCE = "ExecuteOnlyOnce";

        public const string NUMBEROFORDERS = "NumberOfOrders";
        public const string ENTRYPIPSFROMTRADEOPENPRICE = "EntryPipsFromTradeOpenPrice";
        public const string TAKEPROFIPIPS = "TakeProfitPips";

        public const string ENABLEORDERSPACING = "EnableOrderSpacing";
        public const string ORDERSPACINGBASEPIPS = "OrderSpacingBasePips";
        public const string ORDERSPACINGMAXPIPS = "OrderSpacingMaxPips";
        public const string ORDERSPACINGINCPIPS = "OrderSpacingIncrementPips";
        public const string INCREMENTSPACINGAFTER = "IncrementSpacingAfterOrders";

        public const string ENABLEVOLUMEINCREASE = "EnableVolumeIncrease";
        public const string VOLUMEBASE = "VolumeBase";
        public const string VOLUMEMAX = "VolumeMax";
        public const string VOLUMEMULTIPLIER = "VolumeMultiplier";
        public const string USEVOLUMEMULTIPLIER = "UseVolumeMultiplier";
        public const string VOLUMEINCREMENT = "VolumeIncrement";
        public const string INCREASEVOLUMEAFTER = "IncreaseVolumeAfterOrders";


        public IDictionary<string,object> Params { get; set; }
        public string Name { get; set; }
        public List<string> ActivateRules { get; set; }
        public List<string> DeactivateRules { get; set; }
    }
}
