using System.Collections.Generic;
using Niffler.RabbitMQ;
using Niffler.Microservices;

namespace Niffler.Messaging.RabbitMQ
{
    abstract public class IScalableConsumerService : IService
    {
        //If any of the values required from config have not been set then IsInitialised will be set to false in derived classes
        protected bool IsInitialised = true; 

        protected Adapter Adapter;
        protected string ExchangeName;

        protected Publisher Publisher;
        protected Consumer Consumer;
        protected ConsumerAutoScaleManager ConsumerAutoScaleManager;

        public void Run(Adapter adapter)
        {
            if (!IsInitialised) return;
            Adapter = adapter;
            Publisher = new Messaging.RabbitMQ.Publisher(Adapter.GetConnection(), ExchangeName);
            Consumer = new Consumer(Adapter, ExchangeName, GetListeningRoutingKeys());
            Consumer.Init();
            Consumer.MessageReceived += OnMessageReceived;

            adapter.ConsumeAsync(Consumer);

            ConsumerAutoScaleManager = new ConsumerAutoScaleManager(Consumer);
            ConsumerAutoScaleManager.Init();
        }

        private List<RoutingKey> GetListeningRoutingKeys()
        {
            //Set Rule specific routingKeys
            List<RoutingKey> routingKeys = SetListeningRoutingKeys();

            //Listen for any SHUTDOWN Action message on the exchange
            routingKeys.Add(RoutingKey.Create(Messaging.RabbitMQ.Action.SHUTDOWN));

            //Listen for any RESET Action message on the exchange
            routingKeys.Add(RoutingKey.Create(Messaging.RabbitMQ.Action.RESET));

            return routingKeys;
        }
       
        //Cannot guarantee OnMessageReceived() to SHUTDOWN is for this consumer therefore leave to derived class to implement ShutDown() method
        protected void ShutDownConsumers()
        {
            ConsumerAutoScaleManager.ShutDown();
        }

        public abstract void Init();
        public abstract void ShutDown();
        public abstract void Reset();
        protected abstract List<RoutingKey> SetListeningRoutingKeys();
        protected abstract void OnMessageReceived(object sender, MessageReceivedEventArgs e);

    }
}
