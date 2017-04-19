using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace Niffler.Indicators.KeyLevels
{
    class Main
    {

        // This will be called every 10 seconds... Filter to only collect once a day.
        public static void Collect(IAccount Account, Symbol Symbol, MarketSeries MarketSeries)
        {


            Model.KeyLevel KeyLevel = Business.KeyLevels.GetByID(Account.BrokerName, Symbol.Code, MarketSeries.OpenTime.LastValue.Date);


            if (KeyLevel == null & Business.TradingHours.GetStatus(Symbol.Code) == Business.TradingHours.StatusEnum.HasClosedForTheDay)
            {

                dynamic TradingHours = Business.TradingHours.GetByID(Symbol.Code);

                KeyLevel = new Model.KeyLevel();
                KeyLevel.Symbol = Symbol.Code;
                KeyLevel.BrokerName = Account.BrokerName;
                KeyLevel.Date = MarketSeries.OpenTime.LastValue.Date;

                double High = int.MinValue;
                double Low = int.MaxValue;
                double Open = 0;
                double Close = 0;


                //24 hours in 30 minutes...
                for (int i = 0; i <= 48; i++)
                {


                    if (MarketSeries.OpenTime.Last(i).Date.ToUniversalTime().TimeOfDay >= TradingHours.OpenTimeUTC & MarketSeries.OpenTime.Last(i).Date.ToUniversalTime().TimeOfDay < TradingHours.CloseTimeUTC)
                    {
                        if (High < MarketSeries.High.Last(i))
                            High = MarketSeries.High.Last(i);

                        if (Low >= MarketSeries.Low.Last(i))
                            Low = MarketSeries.Low.Last(i);

                        if (MarketSeries.OpenTime.Last(i).ToUniversalTime().TimeOfDay == TradingHours.OpenTimeUTC)
                            Open = MarketSeries.Open.Last(i);

                        if (MarketSeries.OpenTime.Last(i).ToUniversalTime().TimeOfDay == TradingHours.CloseTimeUTC)
                             Close = MarketSeries.Open.Last(i);

                    }

                }


                KeyLevel.High = (decimal)High;
                KeyLevel.Low = (decimal)Low;
                KeyLevel.Open = (decimal)Open;
                KeyLevel.Close = (decimal)Close;
                Business.KeyLevels.Save(KeyLevel);

            }





        }

    }
}
