#region Includes

using System;
using System.Collections.Generic;
using Niffler.Messaging.RabbitMQ;
using Niffler.Messaging.Protobuf;
using RabbitMQ.Client;
using Niffler.Common;
using Niffler.Common.Market;

#endregion

namespace Niffler.Managers {
    public class StateManager : IConsumer {

        private string StrategyId;
        private Dictionary<string, object> State;


        public StateManager(IDictionary<string, string> config) : base(config)
        {
            config.TryGetValue("StrategyId", out string StrategyId);
        }

        public override object Clone()
        {
            return new StateManager(BotConfig);
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
                    value = e.Message.State.Boolvalue;
                    break;
                case Messaging.Protobuf.State.Types.ValueType.String:
                    value = e.Message.State.Stringvalue;
                    break;
                case Messaging.Protobuf.State.Types.ValueType.Double:
                    value = e.Message.State.Doublevalue;
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
            return RoutingKey.Create(Source.WILDCARD, Messaging.RabbitMQ.Action.UPDATESTATE, Event.WILDCARD).ToList(); ;
        }
    }
}