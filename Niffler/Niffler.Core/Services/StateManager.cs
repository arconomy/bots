//#region Includes
//using System.Collections.Generic;
//using Niffler.Messaging.RabbitMQ;
//using Niffler.Messaging.Protobuf;
//using System;
//using Niffler.Core.Config;
//using Niffler.Common;
//#endregion

//// Need to refactor StateManager to store State in a persistent data store
//// The StateManager (or Data Store) should notify interested consumers of changes to state data

//namespace Niffler.Services
//{
//    public class StateManager : IScalableConsumerService
//    {

//        private string StrategyId;
//        private Dictionary<string, object> State;
//        private StrategyConfiguration StrategyConfig;

//        public StateManager(StrategyConfiguration strategyConfig)
//        {
//            StrategyConfig = strategyConfig;
//            State = new Dictionary<string, object>();
//        }

//        public override void Init()
//        {
//            StrategyId = StrategyConfig.StrategyId;
//            if (String.IsNullOrEmpty(StrategyId)) IsInitialised = false;

//            ExchangeName = StrategyConfig.Exchange;
//            if (String.IsNullOrEmpty(ExchangeName)) IsInitialised = false;
//        }

//        protected override void OnMessageReceived(Object o, MessageReceivedEventArgs e)
//        {
//            //Only interested in messages for this Strategy
//            if (e.Message.StrategyId != StrategyId) return;
//            if (!IsInitialised) return;
//            if (e.Message.Type != Niffle.Types.Type.State) return;
//            if (e.Message.State == null) return;

//            object value = null;
//            switch (e.Message.State.ValueType)
//            {
//                case Messaging.Protobuf.State.Types.ValueType.Bool:
//                    value = e.Message.State.BoolValue;
//                    break;
//                case Messaging.Protobuf.State.Types.ValueType.String:
//                    value = e.Message.State.StringValue;
//                    break;
//                case Messaging.Protobuf.State.Types.ValueType.Double:
//                    value = e.Message.State.DoubleValue;
//                    break;
//                case Messaging.Protobuf.State.Types.ValueType.Int:
//                    value = e.Message.State.IntValue;
//                    break;
//                case Messaging.Protobuf.State.Types.ValueType.Datetimelong:
//                    value = e.Message.State.LongValue;
//                    break;
//            }

//            if (value != null && State.ContainsKey(e.Message.State.Key))
//            {
//                State[e.Message.State.Key] = value;
//            }
//            else
//            {
//                State.Add(e.Message.State.Key, value);
//            }

//            Console.WriteLine("****************");
//            foreach (KeyValuePair<string, object> kvp in State)
//            {
//                if(kvp.Key == Niffler.Model.State.OPENTIME || kvp.Key == Niffler.Model.State.CLOSETIME || kvp.Key == Niffler.Model.State.REDUCERISKTIME || kvp.Key == Niffler.Model.State.TERMINATETIME)
//                {
//                    Console.WriteLine(kvp.Key + " = " + Utils.FormatDateTimeWithSeparators(Convert.ToInt64(kvp.Value)));
//                }
//                else
//                {
//                    Console.WriteLine(kvp.Key + " = " + kvp.Value);
//                }
//            }
//        }
            
//        protected override List<RoutingKey> SetListeningRoutingKeys()
//        {
//            //Listen for any messages that update State
//            return RoutingKey.Create(Messaging.RabbitMQ.Action.UPDATESTATE).ToList();
//        }

//        public override void Reset()
//        {
//            State.Clear();
//        }

//        public override void ShutDown()
//        {
//            ShutDownService();
//        }
//    }
//}


////package net.thegreshams.firebase4j.demo;

////import java.io.IOException;
////import java.util.LinkedHashMap;
////import java.util.Map;

////import net.thegreshams.firebase4j.error.FirebaseException;
////import net.thegreshams.firebase4j.error.JacksonUtilityException;
////import net.thegreshams.firebase4j.model.FirebaseResponse;
////import net.thegreshams.firebase4j.service.Firebase;

////import org.codehaus.jackson.JsonParseException;
////import org.codehaus.jackson.map.JsonMappingException;

////public class Demo
////{

////    public static void main(String[] args) throws FirebaseException, JsonParseException, JsonMappingException, IOException, JacksonUtilityException {


////        // get the base-url (ie: 'http://gamma.firebase.com/username')
////    String firebase_baseUrl = null;
////		for(String s : args ) {

////			if(s == null || s.trim().isEmpty() ) continue;
////			if(s.trim().split( "=" )[0].equals( "baseUrl" ) ) {
////				firebase_baseUrl = s.trim().split( "=" )[1];
////			}
////		}
////		if(firebase_baseUrl == null || firebase_baseUrl.trim().isEmpty() ) {
////			throw new IllegalArgumentException( "Program-argument 'baseUrl' not found but required" );
////		}

		
////		// create the firebase
////		Firebase firebase = new Firebase(firebase_baseUrl);


////// "DELETE" (the fb4jDemo-root)
////FirebaseResponse response = firebase.delete();


////// "PUT" (test-map into the fb4jDemo-root)
////Map<String, Object> dataMap = new LinkedHashMap<String, Object>();
////dataMap.put( "PUT-root", "This was PUT into the fb4jDemo-root" );
////		response = firebase.put(dataMap );
////		System.out.println( "\n\nResult of PUT (for the test-PUT to fb4jDemo-root):\n" + response );
////System.out.println("\n");


////// "GET" (the fb4jDemo-root)
////response = firebase.get();
////		System.out.println( "\n\nResult of GET:\n" + response );
////System.out.println("\n");


////// "PUT" (test-map into a sub-node off of the fb4jDemo-root)
////dataMap = new LinkedHashMap<String, Object>();
////		dataMap.put( "Key_1", "This is the first value" );
////		dataMap.put( "Key_2", "This is value #2" );
////		Map<String, Object> dataMap2 = new LinkedHashMap<String, Object>();
////dataMap2.put( "Sub-Key1", "This is the first sub-value" );
////		dataMap.put( "Key_3", dataMap2 );
////		response = firebase.put( "test-PUT", dataMap );
////		System.out.println( "\n\nResult of PUT (for the test-PUT):\n" + response );
////System.out.println("\n");


////// "GET" (the test-PUT)
////response = firebase.get( "test-PUT" );
////		System.out.println( "\n\nResult of GET (for the test-PUT):\n" + response );
////System.out.println("\n");


////// "POST" (test-map into a sub-node off of the fb4jDemo-root)
////response = firebase.post( "test-POST", dataMap );
////		System.out.println( "\n\nResult of POST (for the test-POST):\n" + response );
////System.out.println("\n");


////// "DELETE" (it's own test-node)
////dataMap = new LinkedHashMap<String, Object>();
////		dataMap.put( "DELETE", "This should not appear; should have been DELETED" );
////		response = firebase.put( "test-DELETE", dataMap );
////		System.out.println( "\n\nResult of PUT (for the test-DELETE):\n" + response );
////response = firebase.delete( "test-DELETE");
////		System.out.println( "\n\nResult of DELETE (for the test-DELETE):\n" + response );
////response = firebase.get( "test-DELETE" );
////		System.out.println( "\n\nResult of GET (for the test-DELETE):\n" + response );
		
////	}
	
////}
