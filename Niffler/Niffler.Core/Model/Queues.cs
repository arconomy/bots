using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Niffler.Data
{
    public class Queues
    {

        public enum QueueEnum
        {
            adgile_cloud,
            adgile_ffmpeg,
            adgile_portal,
            adgile_subtitles,
            adgile_programmatic,
            adgile_warehouse,
            adgile_allterminals,
            adgile_scheduledtasks
        }

        public static IIServiceBus Queue(QueueEnum QueueName)
        {

            string Q = QueueName.ToString().Replace("_", "-");
            string ConnectionString_ServiceBus = Niffler.Constants.ServiceBusConnectionString;

            return new ServiceBus(ConnectionString_ServiceBus, Q);

        }

        public static IIServiceBus Queue(string QueueName)
        {

            string ConnectionString_ServiceBus = Niffler.Constants.ServiceBusConnectionString;

            return new ServiceBus(ConnectionString_ServiceBus, QueueName);

        }

        public interface IIServiceBus
        {

            bool Create();

            bool Exists();

            bool Empty();

            long Count();

            bool Delete();

            Dictionary<string, int> List();
             
            bool Send<T>(T Obj, Dictionary<string, object> Properties = null);

            BrokeredMessage Receive();

            t Receive<t>(ref Dictionary<string, object> Properties);

        }
        private class ServiceBus : IIServiceBus
        {

            private string _ConStr;
            private string _ReceivingQueue;

            private int _RepeatDepth = 10;

            public ServiceBus(string ConnectionString, string QueueName)
            {
                _ConStr = ConnectionString;
                _ReceivingQueue = QueueName;

            }

            public bool Exists()
            {

                NamespaceManager NM = NamespaceManager.CreateFromConnectionString(_ConStr);

                foreach (Microsoft.ServiceBus.Messaging.QueueDescription Q in NM.GetQueues())
                {
                    if (Q.Path.ToLower().Contains(_ReceivingQueue))
                    {
                        return true;
                    }
                }

                return false;

            }

            public bool Create()
            {

                NamespaceManager NM = NamespaceManager.CreateFromConnectionString(_ConStr);

                Microsoft.ServiceBus.Messaging.QueueDescription Queue = null;

                if (!NM.QueueExists(_ReceivingQueue))
                {
                    Queue = NM.CreateQueue(_ReceivingQueue);
                }

                return (Queue != null);

            }

            public Dictionary<string, int> List()
            {

                Dictionary<string, int> queueList = new Dictionary<string, int>();

                NamespaceManager NM = NamespaceManager.CreateFromConnectionString(_ConStr);

                foreach (Microsoft.ServiceBus.Messaging.QueueDescription Q in NM.GetQueues())
                {
                    queueList.Add(Q.Path, (int)Q.MessageCount);
                }

                return queueList;

            }

            public long Count()
            {

                NamespaceManager NM = NamespaceManager.CreateFromConnectionString(_ConStr);


                try
                {
                    Microsoft.ServiceBus.Messaging.QueueDescription Queue = NM.GetQueue(_ReceivingQueue);

                    return Queue.MessageCount;

                }
                catch (Exception ex)
                {
                    Debugger.Log(1, "Niffler.Data.SQLServer", ex.ToString());
                    return -1;
                }

            }

            public bool Delete()
            {

                NamespaceManager NM = NamespaceManager.CreateFromConnectionString(_ConStr);

                try
                {
                    NM.DeleteQueue(_ReceivingQueue);
                    return true;
                }
                catch (Exception ex)
                {
                    Debugger.Log(1, "Niffler.Data.SQLServer", ex.ToString());
                    return false;
                }

            }

            public bool Empty()
            {

                Delete();
                Create();

                return Exists();

            }
             

            public bool Send<T>(T Obj, string InternalQueue, string InternalAction, Dictionary<string, object> Properties = null)
            {

                if (Properties == null)
                    Properties = new Dictionary<string, object>();
                if (Properties.ContainsKey("QueueName"))
                    Properties["QueueName"] = InternalQueue;
                else
                    Properties.Add("QueueName", InternalQueue);
                if (Properties.ContainsKey("QueueAction"))
                    Properties["QueueAction"] = InternalAction;
                else
                    Properties.Add("QueueAction", InternalAction);

                return Send(Obj, Properties);

            }


            public bool Send<T>(T Obj, Dictionary<string, object> Properties = null)
            {


                // Normal send to one queue...
                QueueClient QueueClient = QueueClient.CreateFromConnectionString(_ConStr, _ReceivingQueue);

                BrokeredMessage Msg = new BrokeredMessage(Obj);
                object value;

                if (Properties != null)
                {
                    foreach (string Key in Properties.Keys)
                    {
                        value = "";
                        if (Properties.TryGetValue(Key, out value))
                        {
                            Msg.Properties.Add(Key, value.ToString());
                        }

                    }
                }

                try
                {
                    QueueClient.Send(Msg);
                    return true;
                }
                catch (Exception ex)
                {
                    Debugger.Log(1, "Niffler.Data.SQLServer", ex.ToString());
                    return false;
                }

            }



            public t Receive<t>(ref Dictionary<string, object> Properties)
            {

                try
                {

                    TimeSpan Timeout = new TimeSpan(1, 0, 0);
                    QueueClient QueueClient = QueueClient.CreateFromConnectionString(_ConStr, _ReceivingQueue);

                    BrokeredMessage BrokeredMessage = QueueClient.Receive(Timeout);


                    if (BrokeredMessage != null)
                    {
                        //Assign the properties to the Referenced 
                        Properties = (Dictionary<string, object>)BrokeredMessage.Properties;

                        return BrokeredMessage.GetBody<t>();
                    }


                }
                catch
                {
                    return default(t);
                }

                return default(t);
            }





            public BrokeredMessage Receive()
            {

                TimeSpan Timeout = new TimeSpan(24, 0, 0);

                QueueClient QueueClient = QueueClient.CreateFromConnectionString(_ConStr, _ReceivingQueue, ReceiveMode.ReceiveAndDelete);

                for (int iRepeat = 0; iRepeat <= _RepeatDepth; iRepeat++)
                {
                    try
                    {
                        return QueueClient.Receive(Timeout);

                    }
                    catch (MessagingCommunicationException commex)
                    {
                        if (commex.IsTransient)
                        {
                            System.Threading.Thread.Sleep(10000);
                        }
                    }
                }

                return null;

            }

        }

    }


}