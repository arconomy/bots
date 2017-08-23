
using System.Collections.Generic;

namespace Niffler.Messaging.RabbitMQ
{
    //Allows consumers to listen for generic actions
    public enum Action
    {
        WILDCARD = 0,
        UPDATESTATE = 1,    //StateManager listens for this
        NOTIFY = 2,         //ReportingManager listens for this
        PLACEORDERS = 3,    //Niffler cAlgo Client listens for this
        EXECUTEORDERS = 4,  //Niffler cAlgo Client listens for this
        CANCELORDERS = 5,   //Niffler cAlgo Client listens for this
        CLOSEPOSITIONS = 6  //Niffler cAlgo Client listens for this
    }

    //Allows consumers to listen for generic Events
    public enum Event
    {
        WILDCARD = 0,
        ONTICK = 1,
        ONPOSITIONOPENED = 2,
        ONPOSITIONCLOSED = 3
    }

    //Allows consumers to listen for specific Entity (rule) notifications
    public enum Entity
    {
        WILDCARD = 0,
    }

    public class RoutingKey
    {
        private string Entity;
        private string Action;
        private string Event;
       
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
            string eventName;
            switch (eventEnum)
            {
                case RabbitMQ.Event.WILDCARD:
                    eventName = "*";
                    break;
                case RabbitMQ.Event.ONTICK:
                    eventName = "OnTick";
                    break;
                case RabbitMQ.Event.ONPOSITIONOPENED:
                    eventName = "OnPositionOpened";
                    break;
                case RabbitMQ.Event.ONPOSITIONCLOSED:
                    eventName = "OnPositionClosed";
                    break;
                default:
                    eventName = "*";
                    break;
            }
            Event = eventName;
        }

        public void SetEntity(Entity entityEnum)
        {
            string entityName;
            switch (entityEnum)
            {
                case RabbitMQ.Entity.WILDCARD:
                    entityName = "*";
                    break;
                default:
                    entityName = "*";
                    break;
            }
            Entity = entityName;
        }

        public void SetAction(Action actionEnum)
        {
            string actionName;
            switch(actionEnum)
            {
                case RabbitMQ.Action.WILDCARD:
                    actionName = "*";
                    break;
                case RabbitMQ.Action.UPDATESERVICE:
                    actionName = "UpdateService";
                    break;
                case RabbitMQ.Action.UPDATESTATE:
                    actionName = "UpdateState";
                    break;
                default:
                    actionName = "*";
                    break;
            }
            Action = actionName;
        }
        
        public string GetRoutingKey()
        {
            return Entity + "." + Action + "." + Event;
        }

        public List<RoutingKey> getRoutingKeyAsList()
        {
            return new List<RoutingKey> { this };
        }
    }
}
