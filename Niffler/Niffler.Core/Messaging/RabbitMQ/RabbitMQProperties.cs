#region Includes

using RabbitMQ.Client;

#endregion

namespace Niffler.Messaging {
    public static class RabbitMQProperties {
        public static IBasicProperties CreateDefaultProperties(IModel model) {
            var properties = model.CreateBasicProperties();

            properties.Persistent = true;
            properties.ContentType = "application/octet-stream";
            return properties;
        }
    }
}