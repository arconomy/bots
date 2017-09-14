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
        public event EventHandler<StateReceivedEventArgs> StateUpdateReceived;
        private FirebaseClient FireBaseClient;
        private string Path;
        private string BaseUrl;
        private string StrategyId;
        private bool HasStrategyId = true;

        private readonly string Auth = "8MT7N9S0H1rmW39SIDdH53USHCt3MY4CGogAQayg"; // Firebase database Secret

        public StateManager(string path, string strategyId = "")
        {
            if(string.IsNullOrEmpty(strategyId)) HasStrategyId = false;

            Path = path;
            StrategyId = strategyId;
            BaseUrl = StrategyConfiguration.BASEURL;
            FireBaseClient = new FirebaseClient(
              BaseUrl,
              new FirebaseOptions
              {
                  AuthTokenAsyncFactory = () => Task.FromResult(Auth)
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

        public async void GetState()
        {
            if (!HasStrategyId) return;
            var stateData = await FireBaseClient
           .Child(Path + StrategyId)
           .OnceSingleAsync<Object>();

            State state = new State();

            Console.WriteLine(stateData.ToString());

            //OnStateUpdateReceived(new StateReceivedEventArgs()
            //                        {
            //                            State = stateData
            //                        });
        }

        public async void UpdateState(IDictionary<string, object> stateData)
        {
            if (!HasStrategyId) return;
            string JsonStateData = JsonConvert.SerializeObject(stateData);
            await FireBaseClient.Child(Path + StrategyId).PatchAsync(JsonStateData);
        }

        public void ListenForStateUpdates()
        {
            if (!HasStrategyId) return;
            var observable = FireBaseClient
            .Child(Path + StrategyId)
            .AsObservable<Object>()
            .Subscribe(s =>
                {
                    if (s.Object != null)
                    {
                       // Console.WriteLine(s.Key.ToString());
                       // Console.WriteLine(s.Object.ToString());
                        OnStateUpdateReceived(new StateReceivedEventArgs() { StateDataType = StateDataType.ITEM, Key = s.Key.ToString(), Value = s.Object.ToString() });
                    }
                }
            );
        }

        public async void Reset()
        {
            if (!HasStrategyId) return;
            await FireBaseClient.Child(Path + StrategyId).DeleteAsync();
        }

        public void OnStateUpdateReceived(StateReceivedEventArgs e)
        {
            StateUpdateReceived?.Invoke(this, e);
        }
    }
}
