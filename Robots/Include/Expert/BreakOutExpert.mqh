//+------------------------------------------------------------------+
//|                                          BreakOutExpertTrade.mqh |
//|                        Copyright 2016, MetaQuotes Software Corp. |
//|                                             https://www.mql5.com |
//+------------------------------------------------------------------+
#property copyright "Copyright 2016, MetaQuotes Software Corp."
#property link      "https://www.mql5.com"
#property version   "1.00"
//| Include                                                          |
//+------------------------------------------------------------------+
#include <Expert\Expert.mqh>
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
class CBreakOutExpert : public CExpert
  {
protected:
   ulong        m_buy_stop_order_number;                    // buy stop order number
   ulong        m_sell_stop_order_number;                    // sell stop order number

public:
   CBreakOutExpert();
   ~CBreakOutExpert();
   CExpertTrade    *Trade(void)   const { return(m_trade);}
   void SetBuyStopOrder(ulong order_number);
   void SetSellStopOrder(ulong order_number);
   ulong GetSellStopOrder();
   ulong GetBuyStopOrder();
   double GetAdjustedPoint();
  };
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
CBreakOutExpert::CBreakOutExpert()
  {
  }
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
CBreakOutExpert::~CBreakOutExpert()
  {
  }
//+------------------------------------------------------------------+
//+------------------------------------------------------------------+
//| Get Buy Stop Order                                               |
//+------------------------------------------------------------------+
ulong CBreakOutExpert::GetBuyStopOrder()  
   {
      return m_buy_stop_order_number;
   }
   
 //+------------------------------------------------------------------+
 //| Set Buy Stop Order                                               |
 //+------------------------------------------------------------------+
 void CBreakOutExpert::SetBuyStopOrder(ulong order_number)
   {
      m_buy_stop_order_number = order_number;
   }
//+------------------------------------------------------------------+
//| Get Sell Stop Order                                              |
//+------------------------------------------------------------------+
ulong CBreakOutExpert::GetSellStopOrder()  
   {
      return m_sell_stop_order_number;
   }
   
 //+------------------------------------------------------------------+
 //| Set Sell Stop Order                                              |
 //+------------------------------------------------------------------+
 void CBreakOutExpert::SetSellStopOrder(ulong order_number)
   {
      m_sell_stop_order_number = order_number;
   }
//+------------------------------------------------------------------+
//| Get Adjusted Point for Symbol                                              |
//+------------------------------------------------------------------+
double CBreakOutExpert::GetAdjustedPoint(void)  
   {
      return m_adjusted_point;
   }
//+------------------------------------------------------------------+