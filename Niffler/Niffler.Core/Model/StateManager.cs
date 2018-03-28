using Firebase.Database;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Niffler.Model;
using System.Threading.Tasks;
using Niffler.Core.Config;
using System.Collections.Specialized;
using Niffler.Messaging.Protobuf;

namespace Niffler.Core.Model
{
    public class StateManager
    {
        public event EventHandler<StateChangedEventArgs> StateUpdateReceived;
        private FirebaseClient FireBaseClient;
        private string BaseUrl;
        private string StrategyId;
        private bool HasStrategyId = true;

        private readonly string Auth = "8MT7N9S0H1rmW39SIDdH53USHCt3MY4CGogAQayg"; // Firebase database Secret

        public StateManager(string strategyId = "")
        {
            if(string.IsNullOrEmpty(strategyId)) HasStrategyId = false;

            StrategyId = strategyId;
            BaseUrl = StrategyConfiguration.BASEURL;
            FireBaseClient = new FirebaseClient(
              BaseUrl,
              new FirebaseOptions
              {
                  AuthTokenAsyncFactory = () => Task.FromResult(Auth)
              });
        }

        public void SetStrategyId(string strategyId)
        {
            StrategyId = strategyId;
            if (!string.IsNullOrEmpty(strategyId)) HasStrategyId = true;
        }

        //Create a Node and return the Unique ID
        public async Task<string> AddStrategyAsync()
        {
            string stateData = "{ id : }";
            string JsonStateData = JsonConvert.SerializeObject(stateData);
            //Create the State path in Firebase for this strategy and return unique ID for strategy
            FirebaseObject<string> result = await FireBaseClient.Child(StrategyConfiguration.STATEPATH)
                .PostAsync(JsonStateData);
            return result.Key;
        }

        //Use for cloning services and reporting
        public async void GetStateSnapShotAsync()
        {
            if (!HasStrategyId) return;
            var data = await FireBaseClient
           .Child(StrategyConfiguration.STATEPATH + StrategyId)
           .OnceSingleAsync<Object>();
            Console.WriteLine(data.ToString());
        }

        //Use for cloning services and reporting
        public async void GetRulesSnapShotAsync(string ruleName)
        {
            if (!HasStrategyId) return;
            var data = await FireBaseClient
           .Child(GetRulePath(ruleName))
           .OnceSingleAsync<Object>();
            Console.WriteLine(data.ToString());
        }

        //Add or Update a Rule key-value pair
        public async void UpdateRuleStatusAsync(string ruleName, string Key, object value)
        {
            if (!HasStrategyId) return;

            Dictionary<string, object> data = new Dictionary<string, object>
            {
                {Key, value }
            };

            string JsonStateData = JsonConvert.SerializeObject(data);
            await FireBaseClient.Child(GetRulePath(ruleName)).PatchAsync(JsonStateData);
        }

        //Add or Update State key-value pair
        public async void UpdateStateAsync(string Key, object value)
        {
            if (!HasStrategyId) return;

            Dictionary<string, object> data = new Dictionary<string, object>
            {
                {Key, value }
            };

            string JsonStateData = JsonConvert.SerializeObject(data);
            await FireBaseClient.Child(StrategyConfiguration.STATEPATH + StrategyId).PatchAsync(JsonStateData);
        }

        //Add or Update Linked Trade
        public async void UpdateStateLinkedTradeAsync(string linkedTradeLabel, string tradeLabel, Object trade)
        {
            if (!HasStrategyId) return;

            //Save the linked trade against a ruleName so that it can be easily reported on
            Dictionary<string, object> data = new Dictionary<string, Object>
            {
                {tradeLabel, trade }
            };

            string JsonStateData = JsonConvert.SerializeObject(data);
            await FireBaseClient.Child(StrategyConfiguration.STATEPATH 
                                            + StrategyId 
                                            + StrategyConfiguration.TRADESPATH 
                                            + linkedTradeLabel).PatchAsync(JsonStateData);
        }

        //Add or Update Trade
        public async void UpdateStateTradeAsync(string tradeLabel, Object trade)
        {
            if (!HasStrategyId) return;

            Dictionary<string, object> data = new Dictionary<string, Object>
            {
                {tradeLabel, trade }
            };

            string JsonStateData = JsonConvert.SerializeObject(data);
            await FireBaseClient.Child(StrategyConfiguration.STATEPATH
                                                        + StrategyId
                                                        + StrategyConfiguration.TRADESPATH).PatchAsync(JsonStateData);
        }


        //Find all Linked Trades
        public async void FindAllLinkedTradesAsync(string tradeLabel, string FIXTradeId, Action<Trade> executeTrade)
        {
            if (!HasStrategyId) return;

            var linkedTrades = await FireBaseClient.Child(StrategyConfiguration.STATEPATH 
                                                            + StrategyId
                                                            + StrategyConfiguration.TRADESPATH
                                                            + tradeLabel).OnceAsync<Trade>();

           foreach (var trade in linkedTrades)
            {
                Trade t = JsonConvert.DeserializeObject<Trade>(trade.Object.ToString());
                t.Order.PosMaintRptID = FIXTradeId;
                executeTrade(t);
            }
        }

        //Find all Linked Trades excluding linked trade
        public async void FindAllLinkedTradesExcludingAsync(string tradeLabel, string excludeTradeLabel, Action<Trade> executeTrade)
        {
            if (!HasStrategyId) return;

            var linkedTrades = await FireBaseClient.Child(StrategyConfiguration.STATEPATH
                                                            + StrategyId
                                                            + StrategyConfiguration.TRADESPATH
                                                            + tradeLabel).OnceAsync<Trade>();

            foreach (var trade in linkedTrades)
            {
                if (!(((Trade)trade.Object).Order.Label == excludeTradeLabel))
                    executeTrade((Trade)trade.Object);
            }
        }



        //Set initial State Data from JSON
        public async void SetInitialStateAsync(IDictionary<string, object> stateData)
        {
            if (!HasStrategyId) return;
            string JsonStateData = JsonConvert.SerializeObject(stateData);
            await FireBaseClient.Child(StrategyConfiguration.STATEPATH + StrategyId).PatchAsync(JsonStateData);
        }

        //Set initial Activation and Deactivation rules from JSON
        public async void SetActivationRulesAsync(string ruleName, List<string> rules)
        {
            if (!HasStrategyId) return;

            Dictionary<string, object> ruleData = new Dictionary<string, object>
            {
                {RuleConfiguration.ACTIVATERULES, rules }
            };

            string JsonStateData = JsonConvert.SerializeObject(ruleData);
            await FireBaseClient.Child(GetRulePath(ruleName)).PatchAsync(JsonStateData);
        }

        public async void SetDeactivationRulesAsync(string ruleName, List<string> rules)
        {
            if (!HasStrategyId) return;

            Dictionary<string, object> ruleData = new Dictionary<string, object>
            {
                {RuleConfiguration.DEACTIVATERULES, rules }
            };

            string JsonStateData = JsonConvert.SerializeObject(ruleData);
            await FireBaseClient.Child(GetRulePath(ruleName)).PatchAsync(JsonStateData);
        }

        public void ListenForStateUpdates()
        {
            if (!HasStrategyId) return;
            var observable = FireBaseClient
            .Child(StrategyConfiguration.STATEPATH + StrategyId)
            .AsObservable<Object>()
            .Subscribe(s =>
                {
                    if (s.Object != null)
                    {
                       // Console.WriteLine(s.Key.ToString());
                       // Console.WriteLine(s.Object.ToString());
                        OnStateChangeReceived(new StateChangedEventArgs() { StateDataType = StateDataType.ITEM, Key = s.Key.ToString(), Value = s.Object.ToString() });
                    }
                }
            );
        }
        
        public async void ResetAsync()
        {
            if (!HasStrategyId) return;
            await FireBaseClient.Child(StrategyConfiguration.STATEPATH + StrategyId).DeleteAsync();
        }

        public void OnStateChangeReceived(StateChangedEventArgs e)
        {
            StateUpdateReceived?.Invoke(this, e);
        }

        private string GetRulePath(string ruleName)
        {
            return StrategyConfiguration.STATEPATH
                        + StrategyId
                        + StrategyConfiguration.RULESPATH
                        + ruleName;
        }
    }
}
