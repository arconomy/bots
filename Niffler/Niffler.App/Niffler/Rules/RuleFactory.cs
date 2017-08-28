using Niffler.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niffler.Rules
{
    public class RulesFactory
    {
        private List<string> RuleNames;

        public RulesFactory()
        { 
            RuleNames.Add(nameof(OnOpenForTrading));
            RuleNames.Add(nameof(CloseTimeCancelPendingOrders));
            RuleNames.Add(nameof(CloseTimeNoPositionsOpenedReset));
            RuleNames.Add(nameof(CloseTimeNoPositionsRemainOpenReset));
            RuleNames.Add(nameof(CloseTimeSetBotState));
            RuleNames.Add(nameof(CloseTimeSetHardSLToLastPositionEntryWithBuffer));
            RuleNames.Add(nameof(OnPositionClosedInProfitCaptureProfitPositionInfo));
            RuleNames.Add(nameof(OnPositionClosedInProfitSetBreakEvenWithBufferIfActive));
            RuleNames.Add(nameof(OnPositionClosedLastEntryPositionStopLossTriggeredCloseAll));
            RuleNames.Add(nameof(OnPositionClosedReportTrade));
            RuleNames.Add(nameof(OnPositionOpenedCaptureLastPositionInfo));
            RuleNames.Add(nameof(OnTickBreakEvenSLActiveSetLastProfitPositionEntry));
            RuleNames.Add(nameof(OnTickChaseFixedTrailingSL));
            RuleNames.Add(nameof(OpenTimeCapturePrice));
            RuleNames.Add(nameof(OpenTimeCaptureSpike));
            RuleNames.Add(nameof(OpenTimePlaceLimitOrders));
            RuleNames.Add(nameof(OpenTimeSetBotState));
            RuleNames.Add(nameof(OpenTimeUseBollingerBand));
            RuleNames.Add(nameof(ReduceRiskTimeReduceRetraceLevels));
            RuleNames.Add(nameof(ReduceRiskTimeSetBotState));
            RuleNames.Add(nameof(ReduceRiskTimeSetHardSLToLastProfitPositionCloseWithBuffer));
            RuleNames.Add(nameof(ReduceRiskTimeSetTrailingStop));
            RuleNames.Add(nameof(RetracedLevel3PlusReduceHardSLBuffer));
            RuleNames.Add(nameof(RetracedLevel1To2SetBreakEvenSLActive));
            RuleNames.Add(nameof(RetracedLevel1To2SetHardSLToLastProfitPositionEntryWithBuffer));
            RuleNames.Add(nameof(RetracedLevel2To3SetFixedTrailingStop));
            RuleNames.Add(nameof(RetracedLevel2To3SetHardSLToLastProfitPositionEntry));
            RuleNames.Add(nameof(RetracedLevel3PlusReduceHardSLBuffer));
            RuleNames.Add(nameof(RetracedLevel3PlusSetHardSLToLastProfitPositionCloseWithBuffer));
            RuleNames.Add(nameof(StartTrading));
            RuleNames.Add(nameof(EndTrading));
            RuleNames.Add(nameof(TerminateTimeCloseAllPositionsReset));
            RuleNames.Add(nameof(TerminateTimeSetBotState));
        }

        //Create and initialise the Rules
        public List<IRule> CreateRules(BotConfiguration botConfig)
        {
            List<IRule> createdRules = new List<IRule>();

            foreach (RuleConfiguration ruleConfig in botConfig.Rules)
            {
                IRule rule = CreateRule(ruleConfig.Name, botConfig.Config);

                if(rule !=null)
                {
                    rule.Init();
                    createdRules.Add(rule);
                }
                else
                {
                    Console.Write("FAILED to create Rule: " + ruleConfig.Name);
                }
            }
            return createdRules;
        }

        private IRule CreateRule(string ruleName, IDictionary<string,string> botConfig)
        {
            if(RuleNames.Contains(ruleName))
            {
                return (IRule) Activator.CreateInstance(Type.GetType(ruleName),new Object[] {botConfig});
            }
            else
            {
                return null;
            }
        }
    }
}
