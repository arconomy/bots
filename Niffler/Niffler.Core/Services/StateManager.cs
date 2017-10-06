using Firebase.Database;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Niffler.Model;
using System.Threading.Tasks;
using Niffler.Core.Config;
using System.Collections.Specialized;

namespace Niffler.Core.Services
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
        public async void GetStateSnapShot()
        {
            if (!HasStrategyId) return;
            var data = await FireBaseClient
           .Child(StrategyConfiguration.STATEPATH + StrategyId)
           .OnceSingleAsync<Object>();
            Console.WriteLine(data.ToString());
        }

        //Use for cloning services and reporting
        public async void GetRulesSnapShot(string ruleName)
        {
            if (!HasStrategyId) return;
            var data = await FireBaseClient
           .Child(StrategyConfiguration.RULESPATH + StrategyId + ruleName)
           .OnceSingleAsync<Object>();
            Console.WriteLine(data.ToString());
        }

        //Add or Update a Rule key-value pair
        public async void UpdateRuleStatus(string ruleName, string Key, object value)
        {
            if (!HasStrategyId) return;

            Dictionary<string, object> data = new Dictionary<string, object>
            {
                {Key, value }
            };

            string JsonStateData = JsonConvert.SerializeObject(data);
            await FireBaseClient.Child(StrategyConfiguration.RULESPATH + StrategyId + "/" + ruleName).PatchAsync(JsonStateData);
        }

        //Add or Update State key-value pair
        public async void UpdateState(string Key, object value)
        {
            if (!HasStrategyId) return;

            Dictionary<string, object> data = new Dictionary<string, object>
            {
                {Key, value }
            };

            string JsonStateData = JsonConvert.SerializeObject(data);
            await FireBaseClient.Child(StrategyConfiguration.STATEPATH + StrategyId).PatchAsync(JsonStateData);
        }

        //Set initial State Data from JSON
        public async void SetInitialState(IDictionary<string, object> stateData)
        {
            if (!HasStrategyId) return;
            string JsonStateData = JsonConvert.SerializeObject(stateData);
            await FireBaseClient.Child(StrategyConfiguration.STATEPATH + StrategyId).PatchAsync(JsonStateData);
        }

        //Set initial Activation and Deactivation rules from JSON
        public async void SetActivationRules(string ruleName, List<string> rules)
        {
            if (!HasStrategyId) return;

            Dictionary<string, object> ruleData = new Dictionary<string, object>
            {
                {RuleConfiguration.ACTIVATERULES, rules }
            };

            string JsonStateData = JsonConvert.SerializeObject(ruleData);
            await FireBaseClient.Child(StrategyConfiguration.RULESPATH + StrategyId + "/" + ruleName).PatchAsync(JsonStateData);
        }

        public async void SetDeactivationRules(string ruleName, List<string> rules)
        {
            if (!HasStrategyId) return;

            Dictionary<string, object> ruleData = new Dictionary<string, object>
            {
                {RuleConfiguration.DEACTIVATERULES, rules }
            };

            string JsonStateData = JsonConvert.SerializeObject(ruleData);
            await FireBaseClient.Child(StrategyConfiguration.RULESPATH + StrategyId + "/" + ruleName).PatchAsync(JsonStateData);
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

        public async void Reset()
        {
            if (!HasStrategyId) return;
            await FireBaseClient.Child(StrategyConfiguration.STATEPATH + StrategyId).DeleteAsync();
            await FireBaseClient.Child(StrategyConfiguration.RULESPATH + StrategyId).DeleteAsync();
        }

        public void OnStateChangeReceived(StateChangedEventArgs e)
        {
            StateUpdateReceived?.Invoke(this, e);
        }
    }
}
