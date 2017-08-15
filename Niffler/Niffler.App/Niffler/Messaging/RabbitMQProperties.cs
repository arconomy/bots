#region Includes

using RabbitMQ.Client;

#endregion

namespace Niffler.Messaging.AMQP {
    public static class RabbitMQProperties {
        public static IBasicProperties CreateDefaultProperties(IModel model) {
            var properties = model.CreateBasicProperties();

            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.ContentEncoding = "UTF-8";
            return properties;
        }
    }
}