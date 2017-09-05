using System;
using Niffler.Messaging.RabbitMQ;
using System.Collections.Generic;
using System.Collections;
using Niffler.Messaging.Protobuf;
using Niffler.Common;
using Niffler.Core.Strategy;

namespace Niffler.Rules.TradingPeriods
{
    class OnOpenForTrading : IRule
    {
        private DateTimeZoneCalculator DateTimeZoneCalc;
        private TimeSpan OpenTime;
        private string SymbolCode;
        private bool OpenMonday;
        private bool OpenTuesday;
        private bool OpenWednesday;
        private bool OpenThursday;
        private bool OpenFriday;
        private bool OpenSaturday;
        private bool OpenSunday;
        private List<DateTime> OpenDates = new List<DateTime>();

        public OnOpenForTrading(StrategyConfiguration strategyConfig, RuleConfiguration ruleConfig) : base(strategyConfig, ruleConfig) { }

        public override void Init()
        {
            //At a minumum need SymbolCode to determine TimeZone & OpenTime
            if(StrategyConfig.Config.TryGetValue(StrategyConfiguration.EXCHANGE, out SymbolCode)) IsInitialised = false; ;
            if (String.IsNullOrEmpty(SymbolCode)) IsInitialised = false; ;
            if (RuleConfig.Params.TryGetValue(RuleConfiguration.OPENTIME, out object openTime)) IsInitialised = false; ;
            if (TimeSpan.TryParse(openTime.ToString(), out OpenTime)) IsInitialised = false; ;

            //Configure which days of the week to trade
            if (RuleConfig.Params.TryGetValue(RuleConfiguration.OPENWEEKDAYS, out object openWeekDays))
            {
                if (openWeekDays is IEnumerable OpenWeekDaysArray)
                {
                    foreach (object day in OpenWeekDaysArray)
                    {
                        switch (day.ToString())
                        {
                            case "Monday":
                                OpenMonday = true;
                                break;
                            case "Tuesday":
                                OpenTuesday = true;
                                break;
                            case "Wednesday":
                                OpenWednesday = true;
                                break;
                            case "Thursday":
                                OpenThursday = true;
                                break;
                            case "Friday":
                                OpenFriday = true;
                                break;
                            case "Saturday":
                                OpenSaturday = true;
                                break;
                            case "Sunday":
                                OpenSunday = true;
                                break;
                        }
                    }

                }
            }

            //Configure specific dates to trade
            if (RuleConfig.Params.TryGetValue(RuleConfiguration.OPENDATES, out object openDates))
            {
                if (openDates is IEnumerable OpenDatesArray)
                {
                    foreach (object date in OpenDatesArray)
                    {
                        if (DateTime.TryParse(date.ToString(), out DateTime dateTime))
                        {
                            OpenDates.Add(dateTime);
                        }

                    }
                }
            }

            DateTimeZoneCalc = new DateTimeZoneCalculator(SymbolCode);
        }
        
        //Execute the rule logic
        override protected bool ExcuteRuleLogic(Niffle message)
        {
            if (IsTickMessageEmpty(message)) return false;

            DateTime now = DateTime.FromBinary(message.Tick.TimeStamp);

            if (IsOpenDate(now) && IsOpenWeekday(now) && IsOpenTime(now))
            {
                PublishStateUpdate(Data.State.ISOPENTIME, true);
                PublishStateUpdate(Data.State.OPENTIME, message.Tick.TimeStamp);
                IsActive = false;
                return true;
            }
            return false;
        }

        override protected void OnServiceNotify(Niffle message, RoutingKey routingKey)
        {
            //Not Listening for any specific Service Notifications.
            throw new NotImplementedException();
        }

        protected override void OnStateUpdate(Niffle message, RoutingKey routingKey)
        {
            //Not Listening for any specific State Update Notifications.
            throw new NotImplementedException();
        }

        private bool IsOpenTime(DateTime now)
        {
            return DateTimeZoneCalc.IsTimeAfter(now, OpenTime);
        }

        private bool IsOpenDate(DateTime now)
        {
            return OpenDates.Exists(date => date.Date == now.Date);
        }
        
        private bool IsOpenWeekday(DateTime now)
        {
            switch (now.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return OpenMonday;
                case DayOfWeek.Tuesday:
                    return OpenTuesday;
                case DayOfWeek.Wednesday:
                    return OpenWednesday;
                case DayOfWeek.Thursday:
                    return OpenThursday;
                case DayOfWeek.Friday:
                    return OpenFriday;
                case DayOfWeek.Saturday:
                    return OpenSaturday;
                case DayOfWeek.Sunday:
                    return OpenSunday;
                default:
                    return false;
            }
        }

        protected override string GetServiceName()
        {
            return nameof(OnOpenForTrading);
        }

        protected override List<RoutingKey> SetListeningRoutingKeys()
        {
            return RoutingKey.Create(Source.WILDCARD, Messaging.RabbitMQ.Action.WILDCARD, Event.ONTICK).ToList();
        }

        public override void Reset()
        {
            //Better to have the Date Store just listen for resets and archive data for the whole StrategyId
            PublishStateUpdate(Data.State.ISOPENTIME, false);
            PublishStateUpdate(Data.State.OPENTIME, null);
            IsActive = true;
        }
    }
}
