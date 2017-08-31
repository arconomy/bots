
using Newtonsoft.Json;
using System.IO;
using Niffler.Strategy;
using Niffler.Services;
using Niffler.Messaging.RabbitMQ;

namespace Niffler.App
{
    class Program
    {
        public static AppConfiguration LoadJson()
        {
            AppConfiguration strategyConfig;

            using (StreamReader r = new StreamReader("userParams.json"))
            {
                string json = r.ReadToEnd();
                strategyConfig = JsonConvert.DeserializeObject<AppConfiguration>(json);
            }
            return strategyConfig;
        }

        public static void Main()
        {
            //Fetch StrategyConfig requried
            AppConfiguration StrategyConfig = LoadJson();

            //Set up Micro-services Required
            ServicesManager ServicesManager = new ServicesManager(StrategyConfig);
        }
    }


}




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
