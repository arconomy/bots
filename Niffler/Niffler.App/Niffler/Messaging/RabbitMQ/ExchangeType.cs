using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niffler.Messaging.RabbitMQ
{
    public enum ExchangeType { TOPIC = 1, DIRECT = 2, FANOUT = 3 };

    public class Exchange
    {
        public static string GetExchangeType(ExchangeType exchangeType)
        {
            switch (exchangeType)
            {
                case ExchangeType.DIRECT:
                    return "direct";
                case ExchangeType.TOPIC:
                    return "topic";
                case ExchangeType.FANOUT:
                    return "fanout";
                default:
                    return null;
            }
        }
    }
}
