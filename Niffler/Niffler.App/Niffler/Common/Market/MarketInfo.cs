using System;
using cAlgo.API;
using System.Collections.Generic;

namespace Niffler.Common.Market
{
    class MarketTradeTimeInfo
    {
        //CLASS NOT REQUIRED AS THIS DATA SHOULD BE PASSED INTO THE APPLICABLE RULE SERVICE

        private TimeZoneInfo TimeZone;
        private TimeSpan OpenTime; //  Open time for Bot to place new trades (not necessarily same as the actual market open)
        private TimeSpan CloseTime; // Close time for Bot to place new trades (not necessarily same as the actual market close)
        private TimeSpan ReduceRiskTime; // ReduceRisk time for Bot to manage trades (not necessarily same as the actual market close) 
        private TimeSpan TerminateTime; // Terminate Bot activity after this time
        private int CloseAfterMinutes; // Closed for Bot to place new trades after minutes
        private int ReduceRiskAfterMinutes; // ReduceRisk time for Bot to manage trades after minutes
        private int TerminateAfterMinutes; // Terminate Bot activity after minutes
        private Robot Bot;
        private bool UseCloseTime;
        private bool UseReduceRiskTime;
        private bool UseTerminateTime;

        //STATE SHOULD BE PERSISTED IN THE STATEMANAGER

        private bool IsBackTesting;
        private bool IsTradeMonday = true;
        private bool IsTradeTuesday = true;
        private bool IsTradeWednesday = true;
        private bool IsTradeThursday = true;
        private bool IsTradeFriday = true;
        private bool IsTradeSaturday = false;
        private bool IsTradeSunday = false;

        public String MarketName { get; set; }

        //Construtor to initialise with Times
        public MarketTradeTimeInfo(IDictionary<string,string> marketInfoConfig)
        {

            marketInfoConfig.TryGetValue("Market", out string market);
            marketInfoConfig.TryGetValue("OpenTime", out string openTime);
            marketInfoConfig.TryGetValue("CloseTime", out string closeTime);





            marketInfoConfig.TryGetValue("OpenTime", out string openTime);
            marketInfoConfig.TryGetValue("OpenTime", out string openTime);
            marketInfoConfig.TryGetValue("OpenTime", out string openTime);


            InitMarketInfo(bot);
            OpenTime = openTime;
            CloseTime = closeTime;
            UseCloseTime = true;
            ReduceRiskTime = reduceRiskTime;
            UseReduceRiskTime = true;
            TerminateTime = terminateTime;
            UseTerminateTime = true;
        }

        //Construtor to initialise with close, reduce risk and terminate minutes after Open time
        public MarketTradeTimeInfo(Robot bot, TimeSpan openTime, int closeAfterMinutes, int reduceRiskAfterMinutes, int terminateAfterMinutes)
        {
            InitMarketInfo(bot);
            OpenTime = openTime;
            SetMinsAfterOpen(closeAfterMinutes, reduceRiskAfterMinutes, terminateAfterMinutes);
        }

        //Construtor to initialise with default Market Opening, Close and Terminate times
        public MarketTradeTimeInfo(Robot bot)
        {
            InitMarketInfo(bot);
            SetDefaultMarketTimes(bot.Symbol.Code);
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



        private void InitMarketInfo(Robot bot)
        {
            Bot = bot;
            IsBackTesting = Bot.IsBacktesting;
            SetTimeZone();
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
            switch (Bot.Symbol.Code)
            {
                case "UK100":
                    MarketName = "FTSE";
                    TimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
                    break;

                case "GER30":
                    MarketName = "DAX";
                    TimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                    break;

                case "HK50":
                    MarketName = "HSI";
                    TimeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
                    break;
            }
        }

        private DateTime GetTimeNow()
        {
            if (IsBackTesting)
            {
                return Bot.Server.Time;
            }
            else
            {
                return DateTime.UtcNow;
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


        public bool IsTradingDay(DayOfWeek day)
        {
            switch(day)
            {
                case DayOfWeek.Monday:
                    return IsTradeMonday;
                case DayOfWeek.Tuesday:
                    return IsTradeTuesday;
                case DayOfWeek.Wednesday:
                    return IsTradeWednesday;
                case DayOfWeek.Thursday:
                    return IsTradeThursday;
                case DayOfWeek.Friday:
                    return IsTradeFriday;
                case DayOfWeek.Saturday:
                    return IsTradeSaturday;
                case DayOfWeek.Sunday:
                    return IsTradeSunday;
                default:
                    return false;
            }
        }

    }
}
