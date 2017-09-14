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
    public class FirebaseManager
    {
        public event EventHandler<StateReceivedEventArgs> StateUpdateReceived;
        private FirebaseClient FireBaseClient;
        private string Path;
        private string BaseUrl;
        private string StrategyId;
        private bool HasStrategyId = true;

        private readonly string Auth = "8MT7N9S0H1rmW39SIDdH53USHCt3MY4CGogAQayg"; // Firebase database Secret

        //Constructor for generic services (not strategy specific)
        public FirebaseManager(string path) : this("",path)
        {
            HasStrategyId = false;
        }
        public FirebaseManager(string strategyId, string path)
        {
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

        public async void GetStateData()
        {
            if (!HasStrategyId) return;
            var stateData = await FireBaseClient
           .Child(Path + StrategyId)
           .OnceSingleAsync<State>();
           
            OnStateUpdateReceived(new StateReceivedEventArgs()
                                    {
                                        State = (State) stateData
                                    });
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
                        Console.WriteLine(s.Key.ToString());
                        Console.WriteLine(s.Object.ToString());
                        OnStateUpdateReceived(new StateReceivedEventArgs() { StateDataType = StateDataType.ITEM, Key = s.Key.ToString(), Value = s.Object.ToString() });
                    }
                }
            );
        }

        public void OnStateUpdateReceived(StateReceivedEventArgs e)
        {
            StateUpdateReceived?.Invoke(this, e);
        }
    }
}
