using Niffler.Core.Strategy;
using Niffler.Rules.TradingPeriods;
using System;
using System.Collections.Generic;

namespace Niffler.Rules
{
    public class RulesFactory
    {
        //Create and initialise the Rules
        public List<IRule> CreateAndInitRules(StrategyConfiguration strategyConfig)
        {
            List<IRule> createdRules = new List<IRule>();

            foreach (RuleConfiguration ruleConfig in strategyConfig.Rules)
            {
                IRule rule = CreateRule(strategyConfig,ruleConfig.Name, ruleConfig);

                if(rule !=null)
                {
                    rule.Init();
                    createdRules.Add(rule);
                    Console.WriteLine("CREATED Rule: " + ruleConfig.Name);
                }
                else
                {
                    Console.WriteLine("FAILED to create Rule: " + ruleConfig.Name);
                }
            }

            return createdRules;
        }

        private IRule CreateRule(StrategyConfiguration strategyConfig, string ruleName, RuleConfiguration ruleConfig)
        {
            if(RuleConfiguration.RuleNames.ContainsKey(ruleName))
            {
                if(RuleConfiguration.RuleNames.TryGetValue(ruleName, out Type ruleType))
                {
                    return (IRule)Activator.CreateInstance(ruleType, new Object[] {strategyConfig,ruleConfig});
                }
                return null;
            }
            return null;
        }
    }
}
