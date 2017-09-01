using System;
using cAlgo.API;
using System.Collections.Generic;
using Niffler.Data;

namespace Niffler.Common.Helpers
{
    class DateTimeZoneCalculator
    {
        private TimeZoneInfo TimeZone;
        private String SymbolCode { get; set; }

        public DateTimeZoneCalculator(string symbolCode)
        {
            this.SymbolCode = symbolCode;
            TimeZone = MarketInfo.GetTimeZone(symbolCode);
        }

        public bool IsTimeBetween(DateTime nowUtc, TimeSpan startTime, TimeSpan endTime)
        {
            DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, TimeZone);
            return (tzTime.TimeOfDay >= startTime && tzTime.TimeOfDay <= endTime);
        }

        public bool IsTimeMoreThanMinsAfter(DateTime nowUtc, int mins, TimeSpan timeToTest)
        {
            DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, TimeZone);
            return (tzTime.TimeOfDay >= timeToTest.Add(TimeSpan.FromMinutes(mins)));
        }

        public bool IsTimeAfter(DateTime nowUtc, TimeSpan timeToTest)
        {
            DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, TimeZone);
            return tzTime.TimeOfDay >= timeToTest;
        }
    }
}
