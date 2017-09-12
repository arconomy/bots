#region Includes

using System;
using RabbitMQ.Client.Events;
using Niffler.Messaging.Protobuf;

#endregion

namespace Niffler.Messaging.RabbitMQ {
    public class MessageReceivedEventArgs : EventArgs
    {
        public Niffle Message { get; set; }
        public BasicDeliverEventArgs EventArgs { get; set; }
        public Exception Exception { get; set; }
    }
}