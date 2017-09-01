using System.Text;
using cAlgo.API;
using RabbitMQ.Client;
using Niffler.Services;
using Niffler.Messaging.RabbitMQ;

namespace NifflerClient

{
    public class TraderService : cAlgo.API.Robot
    {

        //private SellLimitOrdersTrader SellLimitOrdersTrader { get; }
        //private BuyLimitOrdersTrader BuyLimitOrdersTrader { get; }
        //private PositionsManager PositionsManager { get; }
        //private OnTickCaptureSpike SpikeManager { get; }
        //private StopLossManager StopLossManager { get; }
        //private FixedTrailingStop FixedTrailingStop { get; }


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
        private TradeManager TradeManager;


        protected override void OnStart()
        {

            //Set up Adapter to manage single connection for all consumers.
            Adapter Adapter = Adapter.Instance;
            Adapter.Init();
            Adapter.Connect();

            TradeManager = new TradeManager(MarketSeries.SymbolCode);
            TradeManager.Init();
            TradeManager.Run(Adapter);

            //TO DO: TradeManager delgates all the trading message actions to TraderService
            //TradeManager.PlaceBuyLimitOrder += PlaceBuyLimitOrder.

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
    }
}
























