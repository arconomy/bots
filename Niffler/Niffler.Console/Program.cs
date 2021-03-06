﻿using Newtonsoft.Json;
using Niffler.Core.Config;
using System.IO;
using Niffler.Services;

namespace Niffler.App
{
    class Program
    {
        public static JsonAppConfig LoadJson()
        {
            JsonAppConfig appConfig;

            using (StreamReader file = File.OpenText(@"userParams.json"))
            {
                appConfig = JsonConvert.DeserializeObject<JsonAppConfig>(file.ReadToEnd());
            }

            return appConfig;
        }

        public static void Main()
        {
            //Fetch StrategyConfig requried
            JsonAppConfig appConfig = LoadJson();

            //Set up Micro-services Required
            ServicesManager ServicesManager = new ServicesManager(appConfig);
            ServicesManager.Init();
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
