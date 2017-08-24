using System;
using Niffler.Strategy;
using Niffler.Messaging.RabbitMQ;
using System.Collections.Generic;
using Niffler.Common.Helpers;
using System.Collections;
using Niffler.Messaging.Protobuf;

namespace Niffler.Rules.TradingPeriods
{
    class OnOpenForTrading : IRule
    {
        private TimeZoneInfo TimeZone;
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
        DateTime Now;

        public OnOpenForTrading(IDictionary<string, string> botConfig, RuleConfig ruleConfig) : base(botConfig, ruleConfig) { }

        public override bool Init()
        {
            //At a minumum need SymbolCode & OpenTime
            if(BotConfig.TryGetValue("Market", out SymbolCode)) return false;
            if (SymbolCode == "" || SymbolCode == null) return false;
            if (RuleConfig.Params.TryGetValue("OpenTime", out object openTime)) return false;
            if (TimeSpan.TryParse(openTime.ToString(), out OpenTime)) return false;

            //Configure which days of the week to trade
            if (RuleConfig.Params.TryGetValue("OpenWeekDays", out object openWeekDays))
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
            if (RuleConfig.Params.TryGetValue("OpenDates", out object openDates))
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
            return true;
        }
        
        //Get the Opening price for the trading period
        override protected bool ExcuteRuleLogic(Niffle message)
        {
            if (IsTickMessageEmpty(message)) return false;

            if(DateTime.TryParse(message.Tick.Timestamp, out Now))
            {
                if (IsOpenDate() && IsOpenWeekday() && IsOpenTime())
                {
                    IsActive = false;
                    return true;
                }
            }
            return false;
        }

        override protected void OnServiceNotify(Niffle message, RoutingKey routingKey)
        {
            //Not Listening for any other Service Notifcations.
        }

        private bool IsOpenTime()
        {
            return DateTimeZoneCalc.IsTimeAfter(Now,OpenTime);
        }

        private bool IsOpenDate()
        {
            return OpenDates.Exists(date => date.Date == Now.Date);
        }
        
        private bool IsOpenWeekday()
        {
            switch (Now.DayOfWeek)
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

        protected override List<RoutingKey> GetListeningRoutingKeys()
        {
            return RoutingKey.Create(Entity.WILDCARD, Messaging.RabbitMQ.Action.WILDCARD, Event.ONTICK).getRoutingKeyAsList();
        }

        public override object Clone()
        {
            return new OnOpenForTrading(BotConfig, RuleConfig);
        }

        protected override string GetServiceName()
        {
            return nameof(OnOpenForTrading);
        }
    }
}
