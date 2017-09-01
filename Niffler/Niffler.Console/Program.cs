using Newtonsoft.Json;
using Niffler.Core.Strategy;
using System.IO;
using Niffler.Services;

namespace Niffler.App
{
    class Program
    {
        public static AppConfiguration LoadJson()
        {
            AppConfiguration appConfig;

            using (StreamReader file = File.OpenText(@"userParams.json"))
            {
                appConfig = JsonConvert.DeserializeObject<AppConfiguration>(file.ToString());
            }

            return appConfig;
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
