using System;
using cAlgo.API;
using RabbitMQ.Client;
using System.Text;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace NifflerClient

{
    public class Botty : cAlgo.API.Robot
    {
        virtual public DataSeries DataSeriesSource { get; set; }
        virtual public bool UseBollingerBandEntry { get; set; }
        virtual public int BolliEntryPips { get; set; }
        virtual public int TriggerOrderPlacementPips { get; set; }
        virtual public int OrderEntryOffset { get; set; }
        virtual public int OrderSpacing { get; set; }
        virtual public int OrderSpacingLevels { get; set; }
        virtual public double OrderSpacingMultipler { get; set; }
        virtual public int OrderSpacingMax { get; set; }
        virtual public int NumberOfOrders { get; set; }
        virtual public int VolumeBase { get; set; }
        virtual public int VolumeMax { get; set; }
        virtual public int VolumeMultiplierOrderLevels { get; set; }
        virtual public double VolumeMultipler { get; set; }
        virtual public double DefaultTakeProfit { get; set; }
        virtual public int ReduceRiskAfterMins { get; set; }
        virtual public int CloseAfterMins { get; set; }
        virtual public int RetraceLevel1 { get; set; }
        virtual public int RetraceLevel2 { get; set; }
        virtual public int RetraceLevel3 { get; set; }
        virtual public double FinalOrderStopLoss { get; set; }
        virtual public double HardStopLossBuffer { get; set; }
        virtual public double TrailingStopPips { get; set; }

        private IConnection connection;
        private IModel channel;
        private string exchangeName;
        private static bool IsBottyRunning = true;

        protected override void OnStart()
        {
            Positions.Opened += OnPositionOpened;
            Positions.Closed += OnPositionClosed;
            var factory = new ConnectionFactory() { HostName = "localhost", VirtualHost = "nifflermq", UserName = "niffler", Password = "niffler" };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            //Create an exchange per market
            exchangeName = MarketSeries.SymbolCode + "-X";
            channel.ExchangeDeclare(exchange: exchangeName, type: "topic");

            CreateConsumers();
        }

        protected override void OnTick()
        {
            //Send Tick, Positions and Orders
            SendMsg("NoAction.OnTick.NoRule", "OnTick Message");
        }

        protected void OnPositionOpened(PositionOpenedEventArgs args)
        {
            SendMsg("NoAction.OnPositionOpened.NoRule", "OnPositionOpened Message");
        }

        protected void OnPositionClosed(PositionClosedEventArgs args)
        {
            SendMsg("NoAction.OnPositionClosed.NoRule", "OnPositionClosed Message");
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
            SendMsg("NoAction.OnStop.NoRule", "OnStop Message");
            IsBottyRunning = false;
            channel.Close();
            connection.Close();
        }

        private void SendMsg(string routingkey, string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: exchangeName, routingKey: routingkey, basicProperties: null, body: body);
            Print(" [x] sent '{0}':'{1}'", routingkey, message);
        }

        private void CreateConsumers()
        {
            // Define and run consumers to listen for trade msgs
            Task OpenPositionsConsumer = Task.Run(() => ConsumeMessages("OpenPositions.*"));
            Task PlaceBuyLimitTradesConsumer = Task.Run(() => ConsumeMessages("PlaceBuyLimitTrades.*"));
            Task ModifyPositionsConsumer = Task.Run(() => ConsumeMessages("ModifyPositions.*"));
            Task CloseAllPositionsConsumer = Task.Run(() => ConsumeMessages("CloseAllPositions.*"));
        }


        private void ConsumeMessages(string routingkey)
        {
            var queueName = channel.QueueDeclare();
            channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingkey);

            while (IsBottyRunning)
            {
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;
                    Print(" [x] Received '{0}':'{1}'", routingKey, message);
                };
                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            }
            Print(" [x] Stub Consumer Stopped");
        }
    }
}
























