#region Includes
using System.Collections.Generic;
using Niffler.Messaging.RabbitMQ;
using Niffler.Messaging.Protobuf;
using Niffler.Strategy;
#endregion

namespace Niffler.Services{
    public class StateManager : Consumer {

        private string StrategyId;
        private Dictionary<string, object> State;
        private StrategyConfiguration StrategyConfig;

        public StateManager(StrategyConfiguration strategyConfig) : base(strategyConfig.Config)
        {
            StrategyConfig = strategyConfig;
            StrategyConfig.Config.TryGetValue(StrategyConfiguration.STRATEGYID, out StrategyId);
        }

        public override object Clone()
        {
            return new StateManager(StrategyConfig);
        }

        public override bool Init()
        {
            //Initialise connection to data store (firebase) and registers for updates.
            State = new Dictionary<string, object>();
            return true;
        }

        public override void MessageReceived(MessageReceivedEventArgs e)
        {
            if (e.Message.Type != Niffle.Types.Type.State) return;
            if (e.Message.State == null) return;

            object value = null;
            switch (e.Message.State.Valuetype)
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
            }

            if (value != null && State.ContainsKey(e.Message.State.Key))
            {
                State[e.Message.State.Key] = value;
            }
            else
            {
                State.Add(e.Message.State.Key, value);
            }
        }
            
        protected override List<RoutingKey> GetListeningRoutingKeys()
        {
            //Listen for any messages that update State
            return RoutingKey.Create(Messaging.RabbitMQ.Action.UPDATESTATE).ToList();
        }
    }
}