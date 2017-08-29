using System;
using System.Collections.Generic;
using Niffler.Messaging.Protobuf;
using Niffler.Messaging.RabbitMQ;
using Niffler.Strategy;
using Niffler.Common;

namespace Niffler.Managers
{
    class ReportManager : IConsumer
    {
        string StrategyName;
        string StrategyId;
        string Market;
        private StrategyConfiguration StrategyConfig;

        private double ProfitTotal;
        private double PipsTotal;
        private int PositionsOpenedCount;
        private int OrdersPlacedCount;
        private int RulesExecutedCount;
        private int ErrorCount;
        private List<string> TradeExecutionReport = new List<string>();
        private List<string> StrategySummaryReport = new List<string>();
        private List<string> StrategyExecutionReport = new List<string>();
        private string ReportDirectory;
        private string ReportFile;

        public ReportManager(StrategyConfiguration strategyConfig) : base(strategyConfig.Config)
        {
            StrategyConfig = strategyConfig;
            StrategyName = strategyConfig.Name;
        }

        public override object Clone()
        {
            return new ReportManager(StrategyConfig);
        }

        public override bool Init()
        {
            if (!StrategyConfig.Config.TryGetValue(StrategyConfiguration.STRATEGYID, out StrategyId)) return false;
            if (!StrategyConfig.Config.TryGetValue(StrategyConfiguration.MARKET, out Market)) return false;
            ReportDirectory = "C:\\Users\\alist\\Desktop\\" + StrategyName;
            ReportFile = "C:\\Users\\alist\\Desktop\\" + StrategyName + "\\" + StrategyName + "-" + Market + "-" + StrategyId + "-" + Utils.GetTimeStamp(true) + ".csv";

            //Write Reports
            GenerateStrategySummaryReport();
           
            WriteCSVFile(StrategySummaryReport);
            WriteCSVFile(TradeExecutionReport);
            WriteCSVFile(StrategyExecutionReport);

            // reset reporting variables
            ProfitTotal = 0;
            PipsTotal = 0;
            PositionsOpenedCount = 0;
            OrdersPlacedCount = 0;
            RulesExecutedCount = 0;
            ErrorCount = 0;

            TradeExecutionReport.Clear();
            TradeExecutionReport.Add(GetTradeExecutionReportHeaders());
            StrategySummaryReport.Clear();
            StrategySummaryReport.Add(GetStrategySummaryReportHeaders());
            StrategyExecutionReport.Clear();
            StrategyExecutionReport.Add(GetStrategyExecutionReportHeaders());

            return true;
        }

        public override void MessageReceived(MessageReceivedEventArgs e)
        {
            DateTime niffleTimeStamp = DateTime.FromBinary(e.Message.Timestamp);
            RoutingKey routingKey = new RoutingKey(e.EventArgs.RoutingKey);
            string source = routingKey.GetSource();
            string action = routingKey.GetAction();
            string _event = routingKey.GetEvent();

            //Service messages will be notify, shutdown or reset
            if (e.Message.Type == Niffle.Types.Type.Service)
            {
                if (e.Message.Service != null)
                {
                    if (e.Message.Service.Success)
                    {
                        ReportExecution(source, action, _event, Utils.FormatDateTimeWithSeparators(niffleTimeStamp));
                    }
                }
            }

            //Positions messages will be for Position Opened, Closed or Modified.
            if (e.Message.Type == Niffle.Types.Type.Positions)
            {
                if (e.Message.Positions != null)
                {
                    Messaging.Protobuf.Positions positions = e.Message.Positions;
                    for (int count = 0; count < positions.Count; count++)
                    {
                        if (routingKey.GetEventAsEnum() == Event.ONPOSITIONCLOSED)
                        {
                            if (positions.Position[count].StateChange == Messaging.Protobuf.Position.Types.StateChange.Closed)
                            {
                                ReportPositionClosed(positions.Position[count]);
                            }
                        }

                        if (routingKey.GetEventAsEnum() == Event.ONPOSITIONOPENED)
                        {
                            if (positions.Position[count].StateChange == Messaging.Protobuf.Position.Types.StateChange.Opened)
                            {
                                ReportPositionOpened(positions.Position[count]);
                            }
                        }

                        if (routingKey.GetEventAsEnum() == Event.ONPOSITIONMODIFIED)
                        {
                            if (positions.Position[count].StateChange == Messaging.Protobuf.Position.Types.StateChange.Modified)
                            {
                                ReportPositionModified(positions.Position[count], Utils.FormatDateTimeWithSeparators(niffleTimeStamp));
                            }
                        }
                    }
                }
            }

            //Orders messages will be for Orders Placed, Cancelled or Modified
            if (e.Message.Type == Niffle.Types.Type.Orders)
            {
                if (e.Message.Orders != null)
                {
                    Messaging.Protobuf.Orders orders = e.Message.Orders;
                    for (int count = 0; count < orders.Count; count++)
                    {
                        if (routingKey.GetEventAsEnum() == Event.ONORDERPLACED)
                        {
                            if (orders.Order[count].StateChange == Messaging.Protobuf.Order.Types.StateChange.Placed)
                            {
                                ReportOrderPlaced(orders.Order[count]);
                            }
                        }

                        if (routingKey.GetEventAsEnum() == Event.ONORDERCANCELLED)
                        {
                            if (orders.Order[count].StateChange == Messaging.Protobuf.Order.Types.StateChange.Cancelled)
                            {
                                ReportOrderCancelled(orders.Order[count], Utils.FormatDateTimeWithSeparators(niffleTimeStamp));
                            }
                        }

                        if (routingKey.GetEventAsEnum() == Event.ONORDERMODIFIED)
                        {
                            if (orders.Order[count].StateChange == Messaging.Protobuf.Order.Types.StateChange.Modified)
                            {
                                ReportOrderModified(orders.Order[count], Utils.FormatDateTimeWithSeparators(niffleTimeStamp));
                            }
                        }
                    }
                }
            }

            //Error messages
            if (e.Message.Type == Niffle.Types.Type.Orders)
            {
                if (e.Message.Error != null)
                {
                    ReportError(source, e.Message.Error, Utils.FormatDateTimeWithSeparators(niffleTimeStamp));

                }
            }
        }

        public void ReportExecution(string source, string action, string _event, string timestamp)
        {
            RulesExecutedCount++;
            StrategyExecutionReport.Add(timestamp + "," + source + "," + action + "," + _event);
        }

        public void ReportPositionOpened(Messaging.Protobuf.Position position)
        {
            PositionsOpenedCount++;
            StrategyExecutionReport.Add(position.EntryTime + "," + position.Label + "," + "*" + "," + "OnPositionOpened");
        }

        public void ReportPositionClosed(Messaging.Protobuf.Position position)
        {
            PipsTotal += position.Pips;
            ProfitTotal += position.GrossProfit;


            StrategyExecutionReport.Add(position.CloseTime + "," + position.Label + "," + "*" + "," + "OnPositionClosed");
            
            //Add the Rules to the TradeResult report to see which rules executed prior to a position closing
            TradeExecutionReport.Add(position.Label + ","
                                    + position.SymbolCode + ","
                                    + position.EntryTime + ","
                                    + position.CloseTime + ","
                                    + position.EntryPrice + ","
                                    + position.ClosePrice + ","
                                    + position.Pips + ","
                                    + position.StopLossPrice + ","
                                    + position.TakeProfitPrice + ","
                                    + position.GrossProfit);

        }

        public void ReportPositionModified(Messaging.Protobuf.Position position, string timestamp)
        {
            StrategyExecutionReport.Add(timestamp + "," + position.Label + "," + "*" + "," + "OnPositionModified");
        }

        public void ReportOrderPlaced(Messaging.Protobuf.Order order)
        {
            OrdersPlacedCount++;
            StrategyExecutionReport.Add(order.EntryTime + "," + order.Label + "," + "*" + "," + "OnOrderPlaced");
        }

        public void ReportOrderCancelled(Messaging.Protobuf.Order order, string timestamp)
        {
            StrategyExecutionReport.Add(timestamp + "," + order.Label + "," + "*" + "," + "OnOrderCancelled");
        }

        public void ReportOrderModified(Messaging.Protobuf.Order order, string timestamp)
        {
            StrategyExecutionReport.Add(timestamp + "," + order.Label + "," + "*" + "," + "OnOrderModified");
        }

        public void ReportError(string source, Messaging.Protobuf.Error error, string timestamp)
        {
            ErrorCount++
            StrategyExecutionReport.Add(timestamp + "," + source + "," + "*" + "," + "OnError" + "," + error.Message);
        }

        protected override List<RoutingKey> GetListeningRoutingKeys()
        {
            List<RoutingKey> routingKeys = RoutingKey.Create(Messaging.RabbitMQ.Action.NOTIFY).ToList();
            routingKeys.Add(RoutingKey.Create(Event.ONPOSITIONCLOSED));
            routingKeys.Add(RoutingKey.Create(Event.ONPOSITIONOPENED));
            routingKeys.Add(RoutingKey.Create(Event.ONORDERPLACED));
            routingKeys.Add(RoutingKey.Create(Event.ONORDERCANCELLED));
            routingKeys.Add(RoutingKey.Create(Event.ONERROR));
            return routingKeys;
        }

        private string GetTradeExecutionReportHeaders()
        {
            return "Label" + ","
                + "SymbolCode" + ","
                + "EntryTime" + ","
                + "CloseTime" + ","
                + "EntryPrice" + ","
                + "ClosePrice" + ","
                + "Pips" + ","
                + "StopLossPrice" + ","
                + "TakeProfitPrice" + ","
                + "GrossProfit" + ","
                + "PipsTotal" + ","
                + "ProfitTotal";
        }

        private string GetStrategyExecutionReportHeaders()
        {
            return "DateTime" + "," 
                + "Source" + "," 
                + "Action" + "," 
                + "Event" + "," 
                + "Comments";
        }

        private string GetStrategySummaryReportHeaders()
        {
            return "DateTime" + ","
                + "Day of Week" + ","
                + "Orders Placed" + ","
                + "Positions Opened" + ","
                + "Gross Profit" + ","
                + "Total Pips" + ","
                + "Rules Executed" + ","
                + "Errors" + ","
                + "Spike Direction" + ","
                + "Spike Peak Pips";
        }

        private void GenerateStrategySummaryReport()
        {
            StrategySummaryReport.Add(System.DateTime.Now.Date.ToString() + ","
                + System.DateTime.Now.DayOfWeek.ToString() + ","
                + OrdersPlacedCount + ","
                + PositionsOpenedCount + ","
                + ProfitTotal + ","
                + PipsTotal + ","
                + RulesExecutedCount + ","
                + ErrorCount + ","
                + "TBC" + ","
                + "TBC" + ",");
        }
 
        private void WriteCSVFile(List<String> data)
        {
            if (!System.IO.Directory.Exists(ReportDirectory))
                System.IO.Directory.CreateDirectory(ReportDirectory);

            if(System.IO.File.Exists(ReportFile))
            {
                System.IO.File.AppendAllLines(ReportFile, data.ToArray());
            }
            else
            {
                System.IO.File.WriteAllLines(ReportFile, data.ToArray());
            }
        }
    }
}
