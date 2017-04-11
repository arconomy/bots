//+------------------------------------------------------------------+
//|                                                  GeneratedEA.mq5 |
//|                        Copyright 2016, MetaQuotes Software Corp. |
//|                                             https://www.mql5.com |
//+------------------------------------------------------------------+
#property copyright "Copyright 2016, MetaQuotes Software Corp."
#property link      "https://www.mql5.com"
#property version   "1.00"
//+------------------------------------------------------------------+
//| Include                                                          |
//+------------------------------------------------------------------+
#include <Expert\BreakOutExpert.mqh>
//--- available trailing
#include <Expert\Trailing\TrailingFixedPips.mqh>
//--- available money management
#include <Expert\Money\MoneyFixedLot.mqh>
//+------------------------------------------------------------------+
//| Inputs                                                           |
//+------------------------------------------------------------------+
//--- inputs for expert
input string             Expert_Title                  ="GeneratedEA"; // Document name
ulong                    Expert_MagicNumber            =27427;         // 
bool                     Expert_EveryTick              =true;         //

//--- inputs for Default BreakOut
input double             BreakOut_StopLevel            = 10;          // Default Stop Loss level (in points)
input double             BreakOut_ProfitLevel          = 30;        // Default Take Profit level (in points)
input int                BreakOutLevel                 = 70;         // Default breakout level from current price
input int                ModifyPeriod                  = 10;           // Default time to modify orders

/* -- inputs for Signal
input int                Signal_ThresholdOpen          =10;          // Signal threshold value to open [0...100]
input int                Signal_ThresholdClose         =10;          // Signal threshold value to close [0...100]
input double             Signal_PriceLevel             =0.0;         // Price level to execute a deal
input double             Signal_StopLevel              =50.0;        // Stop Loss level (in points)
input double             Signal_TakeLevel              =100.0;        // Take Profit level (in points)
input int                Signal_Expiration             =4;           // Expiration of pending orders (in bars)
*/

//--- inputs for trailing
input int                Trailing_FixedPips_StopLevel  = 30;            // Stop Loss trailing level (in points)
input int                Trailing_FixedPips_ProfitLevel= 100;            // Take Profit trailing level (in points)
//--- inputs for money
input double             Money_FixLot_Percent          =10.0;          // Percent
input double             Money_FixLot_Lots             =0.1;           // Fixed volume
//+------------------------------------------------------------------+
//| Global expert object                                             |
//+------------------------------------------------------------------+
CBreakOutExpert ExtExpert;

//+------------------------------------------------------------------+
//| Global Variables                                                 |
//+------------------------------------------------------------------+


//+------------------------------------------------------------------+
//| Initialization function of the expert                            |
//+------------------------------------------------------------------+
int OnInit()
  {
//--- Initializing expert
Print("*** INIT EXPERT ***");
if(!ExtExpert.Init(Symbol(),Period(),Expert_EveryTick,Expert_MagicNumber))
     {
      //--- failed
      printf(__FUNCTION__+": error initializing expert");
      ExtExpert.Deinit();
      return(INIT_FAILED);
     }
     
//--- create timer
   ExtExpert.OnTimerProcess(true);
   EventSetTimer(ModifyPeriod);

//--- Creating signal
   CExpertSignal *signal=new CExpertSignal;
   if(signal==NULL)
     {
      //--- failed
      printf(__FUNCTION__+": error creating signal");
      ExtExpert.Deinit();
      return(INIT_FAILED);
     }
/*---
   ExtExpert.InitSignal(signal);
   signal.ThresholdOpen(Signal_ThresholdOpen);
   signal.ThresholdClose(Signal_ThresholdClose);
   signal.PriceLevel(Signal_PriceLevel);
   signal.StopLevel(Signal_StopLevel);
   signal.TakeLevel(Signal_TakeLevel);
   signal.Expiration(Signal_Expiration);
*/

//--- Creation of trailing object
  CTrailingFixedPips *trailing=new CTrailingFixedPips;
   if(trailing==NULL)
     {
      //--- failed
      printf(__FUNCTION__+": error creating trailing");
      ExtExpert.Deinit();
      return(INIT_FAILED);
     }
//--- Add trailing to expert (will be deleted automatically))
   if(!ExtExpert.InitTrailing(trailing))
     {
      //--- failed
      printf(__FUNCTION__+": error initializing trailing");
      ExtExpert.Deinit();
      return(INIT_FAILED);
     }
//--- Set trailing parameters
trailing.StopLevel(Trailing_FixedPips_StopLevel);
trailing.ProfitLevel(Trailing_FixedPips_ProfitLevel);
//--- Creation of money object
CMoneyFixedLot *money=new CMoneyFixedLot;
   if(money==NULL)
     {
      //--- failed
      printf(__FUNCTION__+": error creating money");
      ExtExpert.Deinit();
      return(INIT_FAILED);
     }
//--- Add money to expert (will be deleted automatically))
   if(!ExtExpert.InitMoney(money))
     {
      //--- failed
      printf(__FUNCTION__+": error initializing money");
      ExtExpert.Deinit();
      return(INIT_FAILED);
     }
//--- Set money parameters
money.Percent(Money_FixLot_Percent);
money.Lots(Money_FixLot_Lots);
//--- Check all trading objects parameters
   if(!ExtExpert.ValidationSettings())
     {
      //--- failed
      ExtExpert.Deinit();
      return(INIT_FAILED);
     }
//--- Tuning of all necessary indicators
   if(!ExtExpert.InitIndicators())
     {
      //--- failed
      printf(__FUNCTION__+": error initializing indicators");
      ExtExpert.Deinit();
      return(INIT_FAILED);
     }
//--- ok
   return(INIT_SUCCEEDED);
  }
//+------------------------------------------------------------------+
//| Deinitialization function of the expert                          |
//+------------------------------------------------------------------+
void OnDeinit(const int reason)
  {
   ExtExpert.Deinit();
  }
//+------------------------------------------------------------------+
//| "Tick" event handler function                                    |
//+------------------------------------------------------------------+
void OnTick()
  {
   ExtExpert.OnTick();
   
  }
//+------------------------------------------------------------------+
//| "Trade" event handler function                                   |
//+------------------------------------------------------------------+
void OnTrade()
  {
   ExtExpert.OnTrade();
  }
//+------------------------------------------------------------------+
//| "Timer" event handler function                                   |
//+------------------------------------------------------------------+
void OnTimer()
  {
   ExtExpert.OnTimer();
   
   bool modifyBuyStop=true;
   bool modifySellStop=true;
   
/*
| Modify Buy Stop and Sell Stop Positions to ensure they are only triggered when market moves quickly
*/
      int _positionTotal=PositionsTotal();
      for(int positionCnt=0; _positionTotal>positionCnt; positionCnt++)
        {
        
         ulong positionTicket=PositionGetTicket(positionCnt);
         PositionSelectByTicket(positionTicket);
               
        //Check whether Expert owns (opened) the position
        if(Expert_MagicNumber>=0 && Expert_MagicNumber!=PositionGetInteger(POSITION_MAGIC)) continue;
                       
         if(PositionGetInteger(POSITION_TYPE)==POSITION_TYPE_BUY)
           {
            modifyBuyStop=false;
            // Set a flag for the Buy Position for the Symbol to not modify as Position is active. 
           }

         if(PositionGetInteger(POSITION_TYPE)==POSITION_TYPE_SELL)
           {
            modifySellStop=false;
            // Set a flag for the Sell Position for the Symbol to not modify as Position is active. 
           }
        }

// Modify Buy Positions for all Symbols that have not been triggered
   if(modifyBuyStop)
     {
      //--- Set the current Buy price and Modify Buy Stop
      SetBuyPrice();
      ModifyBuyStop();
     }

// Modify Buy Positions for all Symbols that have not been triggered
   if(modifySellStop)
     {
      //--- Set current Sell price and Modify Sell Stop   
      SetSellPrice();
      ModifySellStop();
     }  
  }
  //+------------------------------------------------------------------+
//| Place Buy Stop Trade function                                    |
//+------------------------------------------------------------------+
void PlaceBuyStop()
  {
   CExpertTrade *_trader = ExtExpert.Trade();
   
   double _buyPrice = SetBuyPrice();
   double _TP= _buyPrice + BreakOut_ProfitLevel * ExtExpert.GetAdjustedPoint();
   double _SL = _buyPrice - BreakOut_StopLevel * ExtExpert.GetAdjustedPoint();
   //Hack to zero initial Stop Loss and profit level to 0
   //_TP = 0.0;
   //_SL = 0.0;
     
   if(_trader.BuyStop(Money_FixLot_Lots,_buyPrice,Symbol(),_SL,_TP,ORDER_TIME_DAY))
     {
      // Capture Order Number (Not async mode)
      ExtExpert.SetBuyStopOrder((ulong)_trader.ResultOrder());
      Print("*** PLACED BuyStop #",ExtExpert.GetBuyStopOrder());
     }
   else
     {
      Print("FAILED to place Buy Stop Trade.",
            " Return Code Description: ",_trader.ResultRetcodeDescription());
     };
  }
//+------------------------------------------------------------------+
//| Place Sell Stop Trade function                                   |
//+------------------------------------------------------------------+
void PlaceSellStop()
  {
   CExpertTrade *_trader = ExtExpert.Trade();
   
   double _sellPrice = SetSellPrice();
   double _TP=_sellPrice - BreakOut_ProfitLevel* ExtExpert.GetAdjustedPoint();;
   double _SL = _sellPrice + BreakOut_StopLevel * ExtExpert.GetAdjustedPoint();;
   //Hack to zero initial Stop Loss and profit level to 0
   //_TP = 0.0;
   //_SL = 0.0;
   
   if(_trader.SellStop(Money_FixLot_Lots,_sellPrice,Symbol(),_SL,_TP,ORDER_TIME_DAY))
     {
      // Capture Order Number (Not async mode)
      ExtExpert.SetSellStopOrder((ulong)_trader.ResultOrder());
      Print("*** PLACED SellStop #",ExtExpert.GetBuyStopOrder());
     }
   else
     {
      Print("FAILED to place Sell Stop Trade. ",
            ". Return Code Description: ",_trader.ResultRetcodeDescription());
     };
  }
//+------------------------------------------------------------------+
//| Modify Buy Stop Trade function                                   |
//+------------------------------------------------------------------+
void ModifyBuyStop()
  {
   
// Check if the Buy Stop Order is still open                                                          |
   if(OrderSelect(ExtExpert.GetBuyStopOrder()))
     {
      CExpertTrade *_trader = ExtExpert.Trade();

      double _buyPrice = SetBuyPrice();
      double _TP=_buyPrice + BreakOut_ProfitLevel * ExtExpert.GetAdjustedPoint();;
      double _SL = _buyPrice - BreakOut_StopLevel * ExtExpert.GetAdjustedPoint();;
      //Hack to zero initial Stop Loss and profit level to 0
      //_TP = 0.0;
      //_SL = 0.0;
   
      if(_trader.OrderModify(ExtExpert.GetBuyStopOrder(),_buyPrice,_SL,_TP,ORDER_TIME_DAY,0))
        {
         //Print("******* MODIFIED BuyStopOrder: ",GetBuyStopOrder());
        }
      else
        {
         Print("FAILED to Modify Buy Stop Order : #",ExtExpert.GetBuyStopOrder(),
               ". Return Code Description: ",_trader.ResultRetcodeDescription());
        };
     }
   else
     {
     Print("******* NOT FOUND BuyStopOrder #: ",ExtExpert.GetBuyStopOrder()," PLACE NEW BUYSTOP");
      PlaceBuyStop();
      return;
     }
  }
//+------------------------------------------------------------------+
//| Modify Sell Stop Trade function                                  |
//+------------------------------------------------------------------+
void ModifySellStop()
  {
     
// Check if the Sell Stop Order is still open
   if(OrderSelect(ExtExpert.GetSellStopOrder()))
     {
      CExpertTrade *_trader = ExtExpert.Trade();

      double _sellPrice = SetSellPrice();
      double _TP=_sellPrice -BreakOut_ProfitLevel * ExtExpert.GetAdjustedPoint();;
      double _SL = _sellPrice + BreakOut_StopLevel * ExtExpert.GetAdjustedPoint();;
      //Hack to zero initial Stop Loss and profit level to 0
      //_TP = 0.0;
      //_SL = 0.0;
         
      if(_trader.OrderModify(ExtExpert.GetSellStopOrder(),_sellPrice,_SL,_TP,ORDER_TIME_DAY,0))
        {
         // Print("******* MODIFIED SellStopOrder: ",GetSellStopOrder());
        }
      else
        {
         Print("FAILED to modified Sell Stop Order: ",ExtExpert.GetSellStopOrder(),
               ". Return Code Description: ",_trader.ResultRetcodeDescription());
        };
     }
   else
     {
      Print("******* NOT FOUND SellStopOrder #: ",ExtExpert.GetSellStopOrder(), " PLACE NEW SELLSTOP");
      PlaceSellStop();
      return;
     }
  }
  
//+------------------------------------------------------------------+
//| Set sell price                                                   |
//+------------------------------------------------------------------+
double SetSellPrice()
  {
   //--- Normalise price parameters
   int _digits = (int) SymbolInfoInteger(Symbol(),SYMBOL_DIGITS);   //No. of decimal places
   double _point = SymbolInfoDouble(Symbol(),SYMBOL_POINT);         // Point 
  
   double _currentSellPrice=SymbolInfoDouble(Symbol(),SYMBOL_ASK);
   double _sellPrice = _currentSellPrice - BreakOutLevel * _point;
   _sellPrice = NormalizeDouble(_sellPrice,_digits);
   return _sellPrice;
   
   
  }
//+------------------------------------------------------------------+
//| Set buy price                                                    |
//+------------------------------------------------------------------+
double SetBuyPrice()
  {
   //--- Normalise price parameters
   int _digits = (int) SymbolInfoInteger(Symbol(),SYMBOL_DIGITS);   //No. of decimal places
   double _point = SymbolInfoDouble(Symbol(),SYMBOL_POINT);         // Point 
  
   double _currentBuyPrice=SymbolInfoDouble(Symbol(),SYMBOL_BID);
   double _buyPrice = _currentBuyPrice + BreakOutLevel * _point;
   _buyPrice = NormalizeDouble(_buyPrice,_digits);
   return _buyPrice;
  }

 
 
