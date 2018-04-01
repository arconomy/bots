
using System.Collections.Generic;
using System.Linq;

namespace Niffler.Messaging.RabbitMQ
{
    //Allows consumers to listen for generic actions See https://create360.atlassian.net/wiki/spaces/TEAM/pages/80347143/AA1.0+Messaging+Design
    public enum Action
    {
        WILDCARD = 0,
        NOTIFY = 1,          //ReportingManager listens for this
        UPDATESTATE = 2,     //StateManager listens for this           
        TRADEOPERATION = 3,  //Niffler cAlgo Client listens for this
        SHUTDOWN = 4,        //All Rules listen for this event
        RESET = 5,            //All Rules listen for this event
        TRADEMANAGEMENT = 6  //TradeManager Service listens for this
    }

    //Allows consumers to listen for generic Events
    public enum Event
    {
        WILDCARD = 0,
        ONTICK = 1,             //Rules interested in Ticks listen for this
        ONPOSITIONOPENED = 2,   //Rules interested in a Position Opened event listen for this
        ONPOSITIONCLOSED = 3,   //Rules interested in a Position Closed event listen for this
        ONPOSITIONMODIFIED = 4, //Rules interested in a Position Modified event listen for this
        ONORDERPLACED = 5,      //Rules interested in a Order Placed event listen for this
        ONORDERCANCELLED = 6,   //Rules interested in a Order Cancelled event listen for this
        ONORDERMODIFIED = 7,    //Rules interested in a Order Modified event listen for this
        ONERROR = 10
    }

    //Allows consumers to listen for specific Source/Target Entity (rulename) for notifications
    public enum Source
    {
        WILDCARD = 0,
        SERVICEMANAGER = 1,
    }

    public class RoutingKey
    {
        public string Source;
        private string Action;
        private string Event;
        private Dictionary<Source, string> EntityLookup = new Dictionary<Source, string>()
        {
            { RabbitMQ.Source.WILDCARD,"*" },
            { RabbitMQ.Source.SERVICEMANAGER,"ServiceManager" }
        };
        private Dictionary<Action, string> ActionLookup = new Dictionary<Action, string>()
        {
            { RabbitMQ.Action.WILDCARD,"*" },
            { RabbitMQ.Action.UPDATESTATE,"UpdateState" },
            { RabbitMQ.Action.NOTIFY,"Notify" },
            { RabbitMQ.Action.TRADEOPERATION,"TradeOperation" },
            { RabbitMQ.Action.RESET,"Reset" },
            { RabbitMQ.Action.SHUTDOWN,"ShutDown" },

        };
        private Dictionary<Event, string> EventLookup = new Dictionary<Event, string>()
        {
            { RabbitMQ.Event.WILDCARD,"*" },
            { RabbitMQ.Event.ONTICK,"OnTick" },
            { RabbitMQ.Event.ONPOSITIONOPENED,"OnPositionOpened" },
            { RabbitMQ.Event.ONPOSITIONCLOSED,"OnPositionClosed" },
            { RabbitMQ.Event.ONPOSITIONMODIFIED,"OnPositionModified" },
            { RabbitMQ.Event.ONORDERPLACED,"OnOrderPlaced" },
            { RabbitMQ.Event.ONORDERCANCELLED,"OnOrderCancelled" },
            { RabbitMQ.Event.ONORDERMODIFIED,"OnOrderModified" },
            { RabbitMQ.Event.ONERROR,"OnError" }


        };

        public string GetSource()
        {
            return Source;
        }

        public string GetAction()
        {
            return Action;
        }

        public string GetEvent()
        {
            return Event;
        }

        public Event GetEventAsEnum()
        {
            return EventLookup.FirstOrDefault(_event => _event.Value == Event).Key;
        }

        public Action GetActionAsEnum()
        {
            return ActionLookup.FirstOrDefault(action => action.Value == Event).Key;
        }

        public RoutingKey (string routingKey)
        {
            string[] routingKeySplit = routingKey.Split('.');

            if (routingKeySplit.Length != 3)
            {
                System.Console.Out.Write("FAILED to create routing key: " + routingKey);
                return;
            }

            Source = routingKeySplit[0];
            Action = routingKeySplit[1];
            Event = routingKeySplit[2];
        }

        public RoutingKey(Source source = RabbitMQ.Source.WILDCARD, Action actionEnum = RabbitMQ.Action.WILDCARD, Event eventEnum = RabbitMQ.Event.WILDCARD)
        {
            SetSource(source);
            SetAction(actionEnum);
            SetEvent(eventEnum);
        }

        public RoutingKey(string SourceName, Action actionEnum = RabbitMQ.Action.WILDCARD, Event eventEnum = RabbitMQ.Event.WILDCARD)
        {
            //For Rules passing there nameof(class) name 
            Source = SourceName;
            SetAction(actionEnum);
            SetEvent(eventEnum);
        }

        public void SetEvent(Event eventEnum)
        {
            EventLookup.TryGetValue(eventEnum, out Event);
        }

        public void SetSource(Source source)
        {
            EntityLookup.TryGetValue(source, out Source);
        }

        public void SetAction(Action actionEnum)
        {
            ActionLookup.TryGetValue(actionEnum, out Action);
        }
        
        public string GetRoutingKey()
        {
            return Source + "." + Action + "." + Event;
        }

        public static RoutingKey Create(Source source)
        {
            return new RoutingKey(source, RabbitMQ.Action.WILDCARD, RabbitMQ.Event.WILDCARD);
        }

        public static RoutingKey Create(Action actionEnum)
        {
            return new RoutingKey(RabbitMQ.Source.WILDCARD, actionEnum, RabbitMQ.Event.WILDCARD);
        }

        public static RoutingKey Create(Event eventEnum)
        {
            return new RoutingKey(RabbitMQ.Source.WILDCARD, RabbitMQ.Action.WILDCARD, eventEnum);
        }

        public static RoutingKey Create(Source source = RabbitMQ.Source.WILDCARD,Action actionEnum = RabbitMQ.Action.WILDCARD, Event eventEnum = RabbitMQ.Event.WILDCARD)
        {
            return new RoutingKey(source, actionEnum, eventEnum);
        }

        public static RoutingKey Create(string source, Action actionEnum = RabbitMQ.Action.WILDCARD, Event eventEnum = RabbitMQ.Event.WILDCARD)
        {
            return new RoutingKey(source,actionEnum, eventEnum);
        }


        public List<RoutingKey> ToList()
        {
            return new List<RoutingKey> { this };
        }
    }
}
