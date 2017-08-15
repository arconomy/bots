#region Includes

using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Niffler.Messaging.AMQP;
using System;
using System.Threading;

#endregion

namespace Niffler.Messaging.RabbitMQ {
    public class Adapter : IDisposable
    {
        protected string HostName;
        protected string VirtualHost;
        protected int Port;
        protected string UserName;
        protected string Password;
        protected ushort Heartbeat;
        public static readonly Adapter Instance = new Adapter();
        private IConnection Connection;

        static Adapter() { }

        public void ConsumeAsync(Consumer consumer)
        {
            if (!IsConnected) Connect();

            var thread = new Thread(o => consumer.Start(this));
            thread.Start();

            while (!thread.IsAlive)
                Thread.Sleep(1);
        }

        public void StopConsumingAsync(Consumer consumer)
        {
            consumer.Stop();
        }

        void IDisposable.Dispose()
        {
            Disconnect();
        }

        public bool IsConnected
        {
            get { return Connection != null && Connection.IsOpen; }
        }
        
        public Adapter Init()
        {
            this.HostName = System.Configuration.ConfigurationManager.AppSettings["rabbbitmq-host"];
            this.VirtualHost = System.Configuration.ConfigurationManager.AppSettings["rabbbitmq-vhost"];
            this.Port = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["rabbbitmq-port"]);
            this.UserName = System.Configuration.ConfigurationManager.AppSettings["rabbitmq-username"];
            this.Password = System.Configuration.ConfigurationManager.AppSettings["rabbbitmq-password"];
            this.Heartbeat = Convert.ToUInt16(System.Configuration.ConfigurationManager.AppSettings["rabbbitmq-heartbeat"]);
            return this;
        }
            public void Connect()
        {

            //var connectionFactory = new ConnectionFactory {
            //    HostName = hostName,
            //    Port = port,
            //    UserName = userName,
            //    Password = password,
            //    RequestedHeartbeat = heartbeat
            //};

            //if (!string.IsNullOrEmpty(virtualHost)) connectionFactory.VirtualHost = virtualHost;
            //_connection = connectionFactory.CreateConnection();

            var factory = new ConnectionFactory() { HostName = "localhost", VirtualHost = "nifflermq", UserName = "niffler", Password = "niffler" };
            Connection = factory.CreateConnection();
        }

        public void Disconnect()
        {
            if (Connection != null) Connection.Dispose();
        }

        public object GetConnection()
        {
            return Connection;
        }

        public void Publish(string message, string queueName, bool createQueue = true,
            IBasicProperties messageProperties = null, IDictionary<string, object> queueArgs = null)
        {
            if (!IsConnected) Connect();
            using (var channel = Connection.CreateModel())
            {
                if (createQueue) channel.QueueDeclare(queueName, true, false, false, queueArgs);
                var payload = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(string.Empty, queueName,
                    messageProperties ?? RabbitMQProperties.CreateDefaultProperties(channel), payload);
            }
        }

        public void Publish(string message, string exchangeName, string routingKey,
            IBasicProperties messageProperties = null)
        {
            if (!IsConnected) Connect();
            using (var channel = Connection.CreateModel())
            {
                var payload = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchangeName, routingKey,
                    messageProperties ?? RabbitMQProperties.CreateDefaultProperties(channel), payload);
            }
        }
    }
}