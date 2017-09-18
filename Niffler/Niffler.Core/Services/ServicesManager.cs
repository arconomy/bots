using System.Collections.Generic;
using System;
using Niffler.Rules;
using Niffler.Messaging.RabbitMQ;
using Niffler.Core.Config;
using Niffler.Core.Services;

namespace Niffler.Services
{
    public class ServicesManager
    {
        private RulesFactory RulesFactory = new RulesFactory();
        private List<IRule> Rules;
        private List<IScalableConsumerService> Services;
        private JsonAppConfig AppConfig;
        private Adapter Adapter;

        public ServicesManager(JsonAppConfig appConfig)
        {
            AppConfig = appConfig;
            Services = new List<IScalableConsumerService>();
        }

        public void Init()
        {
            //Set up Adapter to manage single connection for all consumers.
            Adapter = Adapter.Instance;
            Adapter.Init();
            Adapter.Connect();

            //Create a FirebaseManager to set up state data mangement
            StateManager StateManager = new StateManager();
            
            //For each StrategyConfig Initialise Rules as micro-services and other default micro-services
            foreach (StrategyConfiguration strategyConfig in AppConfig.StrategyConfig)
            {
                //Generate Strategy ID here and pass to the State and Rules
                strategyConfig.StrategyId = StateManager.AddStrategyAsync().Result.ToString();
                StateManager.SetStrategyId(strategyConfig.StrategyId);
                StateManager.UpdateState("Name", strategyConfig.Name);

                //Create the rule services per strategy
                Rules = (RulesFactory.CreateAndInitRules(strategyConfig));
                Rules.ForEach(rule => rule.Run(Adapter));

                //Create a Report Manager per strategy
                Services.Add(new ReportManager(strategyConfig));

                Services.ForEach(service => service.Init());
                Services.ForEach(service => service.Run(Adapter));
            }
        }

        public void ShutDown()
        {
            Rules.ForEach(rule => rule.ShutDown());
            Services.ForEach(service => service.ShutDown());
        }

        public void Reset()
        {
            Rules.ForEach(rule => rule.Reset());
            Services.ForEach(service => service.Reset());
        }

        static private string GenerateStrategyId()
        {
            Random randomIdGenerator = new Random();
            int id = randomIdGenerator.Next(0, 99999);
            return id.ToString("00000");
        }        
    }
}
