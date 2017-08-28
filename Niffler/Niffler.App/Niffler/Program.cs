using Niffler.Common;
using Niffler.Common.Market;
using Niffler.Common.Trade;
using Niffler.Messaging.RabbitMQ;
using Niffler.Rules;
using RabbitMQ.Client;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using Niffler.Strategy;
using Niffler.Microservices;

namespace Niffler.App
{
    class Program
    {
        public static StrategyConfiguration LoadJson()
        {
            StrategyConfiguration strategyConfig;

            using (StreamReader r = new StreamReader("userParams.json"))
            {
                string json = r.ReadToEnd();
                strategyConfig = JsonConvert.DeserializeObject<StrategyConfiguration>(json);
            }
            return strategyConfig;
        }

        public static void Main()
        {


            //Fetch StrategyConfig requried
            StrategyConfiguration StrategyConfig = LoadJson();

            //Set up Micro-services Required
            ServicesManager ServicesManager = new ServicesManager(adapter.GetConnection(), StrategyConfig);
        }
    }


}


////Set up Messaging..
//var adapter = Adapter.Instance;
//    adapter.Init("localhost", "nifflermq", 15672, "niffler", "niffler", 50);
//    adapter.Connect();

//    var lifeCycleEventsConsumer = ConsumerFactory.CreateConsumer((IConnection) adapter.GetConnection(), "FTSE100X", "topic", "LifeCyleEventsQ",new string[]{"OnStart.*","OnStop.*"}, 10);
//    adapter.ConsumeAsync(lifeCycleEventsConsumer);

//    var positionsOpenedConsumer = ConsumerFactory.CreateConsumer((IConnection)adapter.GetConnection(), "FTSE100X", "topic", "PositionsOpenedQ", new string[] { "OnPositionOpened.*" }, 10);
//    adapter.ConsumeAsync(positionsOpenedConsumer);

//    var positionsClosedConsumer = ConsumerFactory.CreateConsumer((IConnection)adapter.GetConnection(), "FTSE100X", "topic", "PositionsClosedQ", new string[] { "OnPositionClosed.*" }, 10);
//    adapter.ConsumeAsync(positionsClosedConsumer);

//    var ticksConsumer = ConsumerFactory.CreateConsumer((IConnection)adapter.GetConnection(), "FTSE100X", "topic", "TicksQ", new string[] { "OnTick.*" }, 10);
//    adapter.ConsumeAsync(ticksConsumer);

//    Console.WriteLine(" Press [enter] to exit.");
//    Console.ReadLine();
