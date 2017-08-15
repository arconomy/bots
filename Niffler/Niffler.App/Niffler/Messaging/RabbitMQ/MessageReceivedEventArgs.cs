#region Includes

using System;
using RabbitMQ.Client.Events;

#endregion

namespace Niffler.Messaging.RabbitMQ {
    public class MessageReceivedEventArgs : EventArgs {
        public string Message { get; set; }
        public BasicDeliverEventArgs EventArgs { get; set; }
        public Exception Exception { get; set; }
    }
}