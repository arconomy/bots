using Firebase.Database;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Niffler.Model;
using System.Threading.Tasks;
using Niffler.Core.Config;

namespace Niffler.Core.Services
{
    public class FirebaseManager
    {
        public event EventHandler<StateReceivedEventArgs> StateUpdateReceived;
        private FirebaseClient FireBaseClient;
        private string Path;
        private string BaseUrl;

        public FirebaseManager(string path)
        {
            Path = path;
            BaseUrl = StrategyConfiguration.BASEURL;
            var auth = "8MT7N9S0H1rmW39SIDdH53USHCt3MY4CGogAQayg"; // Firebase database Secret
            FireBaseClient = new FirebaseClient(
              BaseUrl,
              new FirebaseOptions
              {
                  AuthTokenAsyncFactory = () => Task.FromResult(auth)
              });

        }

        public async Task<string> AddStrategyAsync(string stategyName)
        {
            string stateData = "{ name : " + stategyName + "}";
            string JsonStateData = JsonConvert.SerializeObject(stateData);
            FirebaseObject<string> result = await FireBaseClient.Child(Path)
                .PostAsync(JsonStateData);
            return result.Key;
        }

        public async void GetStateData(string strategyId)
        {
            var stateData = await FireBaseClient
           .Child(Path + strategyId)
           .OnceSingleAsync<State>();
           
            OnStateUpdateReceived(new StateReceivedEventArgs()
                                    {
                                        State = (State) stateData
                                    });
        }

        public async void UpdateState(string strategyId, IDictionary<string, object> stateData)
        {
            string JsonStateData = JsonConvert.SerializeObject(stateData);
            await FireBaseClient.Child(Path + strategyId).PatchAsync(JsonStateData);
        }

        public void ListenForAllStateUpdates(string strategyId)
        {
            var observable = FireBaseClient
            .Child(Path)
            .AsObservable<State>()
            .Subscribe(s => OnStateUpdateReceived(new StateReceivedEventArgs() { StateDataType = StateDataType.FULL, State = (State) s.Object }));
        }

        public void ListenForStateItemUpdates(string strategyId, string key)
        {
            var observable = FireBaseClient
            .Child(Path + strategyId)
            .AsObservable<KeyValuePair<string, object>>()
            .Subscribe(kvp => OnStateUpdateReceived(new StateReceivedEventArgs() { StateDataType = StateDataType.ITEM, Key = kvp.Key, Value = kvp.Object}));
        }

        protected void OnStateUpdateReceived(StateReceivedEventArgs e)
        {
            StateUpdateReceived?.Invoke(this, e);
        }
    }
}
