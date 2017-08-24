using System;
using cAlgo.API;
using System.Collections.Generic;

namespace Niffler.Common.Helpers
{
    class DateTimeZoneCalculator
    {
        private TimeZoneInfo TimeZone;
        private bool IsBackTesting;
        private String SymbolCode { get; set; }

        //Construtor to initialise with Times
        public DateTimeZoneCalculator(string symbolCode)
        {
            this.SymbolCode = symbolCode;
            SetTimeZone();
            IsBackTesting = Bot.IsBacktesting;
        }

        private void SetMinsAfterOpen(int closeAfterMinutes, int reduceRiskAfterMinutes, int terminateAfterMinutes)
        {
            SetCloseAfterMinutes(closeAfterMinutes);
            SetReduceRiskAfterMinutes(reduceRiskAfterMinutes);
            SetTerminateAfterMinutes(terminateAfterMinutes);
        }

        public void SetCloseAfterMinutes(int closeAfterMinutes) { CloseAfterMinutes = closeAfterMinutes; UseCloseTime = false; }
        public void SetReduceRiskAfterMinutes(int reduceRiskAfterMinutes) { ReduceRiskAfterMinutes = reduceRiskAfterMinutes; UseReduceRiskTime = false; }
        public void SetTerminateAfterMinutes(int terminateAfterMinutes) { TerminateAfterMinutes = terminateAfterMinutes; UseTerminateTime = false; }
        public void SetTradingDays(bool mon,bool tues,bool wed,bool thurs,bool fri,bool sat, bool sun)
        {
            IsTradeMonday = mon;
            IsTradeTuesday = tues;
            IsTradeWednesday = wed;
            IsTradeThursday = thurs;
            IsTradeFriday = fri;
            IsTradeSaturday = sat;
            IsTradeSunday = sun;
        }

        private void SetDefaultMarketTimes(string symbolCode)
        {
            switch (symbolCode)
            {
                case "UK100":
                    OpenTime = new TimeSpan(8, 0, 0);
                    CloseTime = new TimeSpan(8, 5, 0);
                    UseCloseTime = true;
                    ReduceRiskTime = new TimeSpan(8, 45, 0);
                    UseReduceRiskTime = true;
                    TerminateTime = new TimeSpan(11, 29, 0);
                    UseTerminateTime = true;
                    break;

                case "GER30":
                    OpenTime = new TimeSpan(9, 0, 0);
                    CloseTime = new TimeSpan(9, 3, 0);
                    UseCloseTime = true;
                    ReduceRiskTime = new TimeSpan(8, 45, 0);
                    UseReduceRiskTime = true;
                    TerminateTime = new TimeSpan(11, 29, 0);
                    UseTerminateTime = true;
                    break;

                case "HK50":
                    OpenTime = new TimeSpan(9, 30, 0);
                    CloseTime = new TimeSpan(9, 35, 0);
                    UseCloseTime = true;
                    ReduceRiskTime = new TimeSpan(10, 15, 0);
                    UseReduceRiskTime = true;
                    TerminateTime = new TimeSpan(12, 0, 0);
                    UseTerminateTime = true;
                    break;
            }
        }

        private void SetTimeZone()
        {
            switch (SymbolCode)
            {
                case "UK100":
                    SymbolCode = "FTSE";
                    TimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
                    break;

                case "GER30":
                    SymbolCode = "DAX";
                    TimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                    break;

                case "HK50":
                    SymbolCode = "HSI";
                    TimeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
                    break;
            }
        }

        //Is the current time within the Open time and the Close time period
        public bool IsBotTradingOpen()
        {

            DateTime dateTimeNow = GetTimeNow();

            if (UseCloseTime)
            {
                return IsTradingDay(dateTimeNow.DayOfWeek) && IsTimeBetweenOpenAnd(dateTimeNow, CloseTime);
            }
            else
            {
                return IsTradingDay(dateTimeNow.DayOfWeek) && IsMinsAfterOpenTime(dateTimeNow, 0) && !IsMinsAfterOpenTime(dateTimeNow, CloseAfterMinutes);
            }
        }

        //Is the current time after the Reduce Risk Time
        public bool IsAfterReduceRiskTime()
        {
            if(UseReduceRiskTime)
            {
                return IsTimeAfter(GetTimeNow(), ReduceRiskTime);
            }
            else
            {
                return IsMinsAfterOpenTime(GetTimeNow(), ReduceRiskAfterMinutes);
            }

        }

        //Is the current time after the Close Time
        public bool IsAfterCloseTime()
        {
            if (UseCloseTime)
            {
                return IsTimeAfter(GetTimeNow(), CloseTime);
            }
            else
            {
                return IsMinsAfterOpenTime(GetTimeNow(), CloseAfterMinutes);
            }
                
        }

        //Is the current time after the terminate activity time
        public bool IsAfterTerminateTime()
        {
            if (UseTerminateTime)
            {
                return IsTimeAfter(GetTimeNow(), TerminateTime);
            }
            else
            {
                return IsMinsAfterOpenTime(GetTimeNow(), TerminateAfterMinutes);
            }
        }

        public bool IsTimeBetweenOpenAnd(DateTime dateTimeUtc, TimeSpan timeToTest)
        {
            DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, TimeZone);
            return (tzTime.TimeOfDay >= OpenTime & tzTime.TimeOfDay <= timeToTest);
        }

        public bool IsMinsAfterOpenTime(DateTime dateTimeUtc, int mins)
        {
            DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, TimeZone);
            return (tzTime.TimeOfDay >= OpenTime.Add(TimeSpan.FromMinutes(mins)));
        }

        public bool IsTimeAfter(DateTime dateTimeUtc, TimeSpan timeToTest)
        {
            DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, TimeZone);
            return tzTime.TimeOfDay >= timeToTest;
        }
    }
}
