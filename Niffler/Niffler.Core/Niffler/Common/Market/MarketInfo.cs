using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace Niffler.Common.Market
{
    class MarketInfo
    {
        private TimeZoneInfo TimeZone;
        private TimeSpan OpenTime; //  Open time for Bot to place new trades (not necessarily same as the actual market open)
        private TimeSpan CloseTime; // Close time for Bot to place new trades (not necessarily same as the actual market close)
        private TimeSpan ReduceRiskTime; // ReduceRisk time for Bot to manage trades (not necessarily same as the actual market close) 
        private TimeSpan TerminateTime; // Terminate Bot activity after this time
        private int CloseAfterMinutes; // Closed for Bot to place new trades after minutes
        private int ReduceRiskAfterMinutes; // ReduceRisk time for Bot to manage trades after minutes
        private int TerminateAfterMinutes; // Terminate Bot activity after minutes
        private bool IsBackTesting;
        private Robot Bot;
        private bool UseCloseTime;
        private bool UseReduceRiskTime;
        private bool UseTerminateTime;

        public String MarketName { get; set; }

        //Construtor to initialise with Times
        public MarketInfo(Robot bot, TimeSpan openTime, TimeSpan closeTime, TimeSpan reduceRiskTime, TimeSpan terminateTime)
        {
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
        public MarketInfo(Robot bot, TimeSpan openTime, int closeAfterMinutes, int reduceRiskAfterMinutes, int terminateAfterMinutes)
        {
            InitMarketInfo(bot);
            OpenTime = openTime;
            SetMinsAfterOpen(closeAfterMinutes, reduceRiskAfterMinutes, terminateAfterMinutes);
        }

        //Construtor to initialise with default Market Opening, Close and Terminate times
        public MarketInfo(Robot bot)
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


        private DateTime getTimeNow()
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
            if(UseCloseTime)
            {
                return IsTimeBetweenOpenAnd(getTimeNow(),CloseTime);
            }
            else
            {
                return IsMinsAfterOpenTime(getTimeNow(),CloseAfterMinutes);
            }
        }

        //Is the current time after the Reduce Risk Time
        public bool IsAfterReduceRiskTime()
        {
            if(UseReduceRiskTime)
            {
                return IsTimeAfter(getTimeNow(), ReduceRiskTime);
            }
            else
            {
                return IsMinsAfterOpenTime(getTimeNow(), ReduceRiskAfterMinutes);
            }

        }

        //Is the current time after the Close Time
        public bool IsAfterCloseTime()
        {
            if (UseCloseTime)
            {
                return IsTimeAfter(getTimeNow(), CloseTime);
            }
            else
            {
                return IsMinsAfterOpenTime(getTimeNow(), CloseAfterMinutes);
            }
                
        }

        //Is the current time after the terminate activity time
        public bool IsAfterTerminateTime()
        {
            if (UseTerminateTime)
            {
                return IsTimeAfter(getTimeNow(), TerminateTime);
            }
            else
            {
                return IsMinsAfterOpenTime(getTimeNow(), TerminateAfterMinutes);
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
