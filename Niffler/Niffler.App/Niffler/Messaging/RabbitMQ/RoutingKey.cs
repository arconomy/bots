
using System.Collections.Generic;

namespace Niffler.Messaging.RabbitMQ
{
    public class RoutingKey
    {
        private string Entity;
        private string Action;
        private string Event;
       
        public RoutingKey(string entityName = "*", string actionName = "*", string eventName = "*", bool useStarAsDefault = true)
        {
            if(useStarAsDefault)
            {
                SetPatternUsingStarAsDefault(entityName, actionName, eventName);
            }
            else
            {
                SetPatternUsingHashAsDefault(entityName, actionName, eventName);
            }
        }

        public void SetEntity(string entityName)
        {
            UseStarWildCardForEmptyString(entityName);
        }

        public void SetAction(string actionName)
        {
            UseStarWildCardForEmptyString(actionName);
        }

        public void SetEvent(string eventName)
        {
            UseStarWildCardForEmptyString(eventName);
        }

        public void SetPatternUsingHashAsDefault(string entityName, string actionName, string eventName)
        {
            this.Entity = UseHashWildCardForEmptyString(entityName);
            this.Action = UseHashWildCardForEmptyString(actionName);
            this.Event = UseHashWildCardForEmptyString(eventName);
        }

        public void SetPatternUsingStarAsDefault(string entityName, string actionName, string eventName)
        {
            this.Entity = UseStarWildCardForEmptyString(entityName);
            this.Action = UseStarWildCardForEmptyString(actionName);
            this.Event = UseStarWildCardForEmptyString(eventName);
        }

        public string getRoutingKey()
        {
            return Entity + "." + Action + "." + Event;
        }

        public List<RoutingKey> getRoutingKeyAsList()
        {
            return new List<RoutingKey> { this };
        }

        private string UseStarWildCardForEmptyString(string value)
        {
            if (value != null || value == "")
                return "*";
            else
                return value;
        }

        private string UseHashWildCardForEmptyString(string value)
        {
            if (value != null || value == "")
                return "#";
            else
                return value;
        }
    }
}
