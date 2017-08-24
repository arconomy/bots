
using System.Collections.Generic;
using System.Linq;

namespace Niffler.Messaging.RabbitMQ
{
    //Allows consumers to listen for generic actions
    public enum Action
    {
        WILDCARD = 0,
        UPDATESTATE = 1,    //StateManager listens for this
        NOTIFY = 2,         //ReportingManager listens for this
        TRADEOPERATION = 3  //Niffler cAlgo Client listens for this
    }

    //Allows consumers to listen for generic Events
    public enum Event
    {
        WILDCARD = 0,
        ONTICK = 1,             //Rules interested in Ticks listen for this
        ONPOSITIONOPENED = 2,   //Rules interested in a Position Opened event listen for this
        ONPOSITIONCLOSED = 3    //Rules interested in a Position Closed event listen for this
    }

    //Allows consumers to listen for specific Entity (rule) notifications
    public enum Entity
    {
        WILDCARD = 0,
    }

    public class RoutingKey
    {
        public string Entity { get; }
        private string Action;
        private string Event;
        private Dictionary<Entity, string> EntityLookup = new Dictionary<Entity, string>()
        {
            { RabbitMQ.Entity.WILDCARD,"*" }
        };
        private Dictionary<Action, string> ActionLookup = new Dictionary<Action, string>()
        {
            { RabbitMQ.Action.WILDCARD,"*" },
            { RabbitMQ.Action.UPDATESTATE,"UpdateState" },
            { RabbitMQ.Action.NOTIFY,"Notify" },
            { RabbitMQ.Action.TRADEOPERATION,"TradeOperation" }

        };
        private Dictionary<Event, string> EventLookup = new Dictionary<Event, string>()
        {
            { RabbitMQ.Event.WILDCARD,"*" },
            { RabbitMQ.Event.ONTICK,"OnTick" },
            { RabbitMQ.Event.ONPOSITIONOPENED,"OnPositionOpened" },
            { RabbitMQ.Event.ONPOSITIONCLOSED,"OnPositionClosed" }
        };

        public RoutingKey (string routingKey)
        {
            string[] routingKeySplit = routingKey.Split('.');

            Entity = routingKeySplit[0];
            Action = routingKeySplit[1];
            Event = routingKeySplit[2];
        }

            public RoutingKey(Entity entityEnum = RabbitMQ.Entity.WILDCARD, Action actionEnum = RabbitMQ.Action.WILDCARD, Event eventEnum = RabbitMQ.Event.WILDCARD)
        {
            SetEntity(entityEnum);
            SetAction(actionEnum);
            SetEvent(eventEnum);
        }

        public RoutingKey(string entityName, Action actionEnum = RabbitMQ.Action.WILDCARD, Event eventEnum = RabbitMQ.Event.WILDCARD)
        {
            //For Rules passing there nameof(class) name 
            Entity = entityName;
            SetAction(actionEnum);
            SetEvent(eventEnum);
        }

        public void SetEvent(Event eventEnum)
        {
            EventLookup.TryGetValue(eventEnum, out Event);
        }

        public void SetEntity(Entity entityEnum)
        {
            EntityLookup.TryGetValue(entityEnum, out Entity);
        }

        public void SetAction(Action actionEnum)
        {
            ActionLookup.TryGetValue(actionEnum, out Action);
        }
        
        public string GetRoutingKey()
        {
            return Entity + "." + Action + "." + Event;
        }

        public static RoutingKey Create(Entity entityEnum = RabbitMQ.Entity.WILDCARD, Action actionEnum = RabbitMQ.Action.WILDCARD, Event eventEnum = RabbitMQ.Event.WILDCARD)
        {
            return new RoutingKey(entityEnum, actionEnum, eventEnum);
        }

        public static RoutingKey Create(string entityName, Action actionEnum = RabbitMQ.Action.WILDCARD, Event eventEnum = RabbitMQ.Event.WILDCARD)
        {
            return new RoutingKey(entityName, actionEnum, eventEnum);
        }


        public List<RoutingKey> getRoutingKeyAsList()
        {
            return new List<RoutingKey> { this };
        }
    }
}
