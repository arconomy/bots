#region Includes
using System.Collections.Generic;
using Niffler.Messaging.RabbitMQ;
using Niffler.Messaging.Protobuf;
using System;
using Niffler.Core.Strategy;
#endregion

// Need to refactor StateManager to store State in a persistent data store
// The StateManager (or Data Store) should notify interested consumers of changes to state data

namespace Niffler.Services
{
    public class StateManager : IScalableConsumerService
    {

        private string StrategyId;
        private Dictionary<string, object> State;
        private StrategyConfiguration StrategyConfig;

        public StateManager(StrategyConfiguration strategyConfig)
        {
            StrategyConfig = strategyConfig;
            State = new Dictionary<string, object>();
        }

        public override void Init()
        {
            StrategyId = StrategyConfig.Config.StrategyId;
            if (String.IsNullOrEmpty(StrategyId)) IsInitialised = false;

            ExchangeName = StrategyConfig.Config.Exchange;
            if (String.IsNullOrEmpty(ExchangeName)) IsInitialised = false;
        }

        protected override void OnMessageReceived(Object o, MessageReceivedEventArgs e)
        {
            //Only interested in messages for this Strategy
            if (e.Message.StrategyId != StrategyId) return;
            if (!IsInitialised) return;
            if (e.Message.Type != Niffle.Types.Type.State) return;
            if (e.Message.State == null) return;

            object value = null;
            switch (e.Message.State.ValueType)
            {
                case Messaging.Protobuf.State.Types.ValueType.Bool:
                    value = e.Message.State.BoolValue;
                    break;
                case Messaging.Protobuf.State.Types.ValueType.String:
                    value = e.Message.State.StringValue;
                    break;
                case Messaging.Protobuf.State.Types.ValueType.Double:
                    value = e.Message.State.DoubleValue;
                    break;
                case Messaging.Protobuf.State.Types.ValueType.Datetime:
                    if (!DateTime.TryParse(e.Message.State.StringValue, out DateTime datetime)) value = e.Message.State.StringValue;
                    break;
            }

            if (value != null && State.ContainsKey(e.Message.State.Key))
            {
                State[e.Message.State.Key] = value;
            }
            else
            {
                State.Add(e.Message.State.Key, value);
            }

            Console.WriteLine("****************");
            foreach (KeyValuePair<string, object> kvp in State)
            {
                Console.WriteLine(kvp.Key + " = " + kvp.Value);
            }


        }
            
        protected override List<RoutingKey> SetListeningRoutingKeys()
        {
            //Listen for any messages that update State
            return RoutingKey.Create(Messaging.RabbitMQ.Action.UPDATESTATE).ToList();
        }

        public override void Reset()
        {
            State.Clear();
        }

        public override void ShutDown()
        {
            ShutDownService();
        }
    }
}