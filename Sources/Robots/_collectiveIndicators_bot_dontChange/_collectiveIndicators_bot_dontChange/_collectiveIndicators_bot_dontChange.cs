////#reference: ..\Indicators\coolective_indic.algo
using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;


/*

close the deal once the candle hits the trend line not on reversal and add trailing stop loss
open the deal only when the 2 trends are going upwards not sideways


*/
namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class _collectiveIndicators_bot_dontChange : Robot
    {

        private coolective_indic _Ci;

        [Parameter(DefaultValue = 0.0001)]
        public double pipValue { get; set; }



        protected override void OnStart()
        {
            _Ci = Indicators.GetIndicator<coolective_indic>(0.0001);
            //Print(_Ci.Result);
        }

        protected override void OnStop()
        {

            foreach (Position p in Positions)
            {
                ClosePosition(p);
            }
            Print("");
        }

        protected override void OnBar()
        {
            double buy = _Ci.buy.LastValue;
            double sell = _Ci.sell.LastValue;
            double reverse = _Ci.reverse.LastValue;


            if (Positions.Count > 0)
            {
                manageOpendPositions();
            }
            else
            {
                if (buy > sell)
                {
                    Open(TradeType.Buy);

                }

                if (buy < sell)
                {
                    Open(TradeType.Sell);

                }
            }


        }

        private void Close(TradeType tradeType)
        {
            foreach (var position in Positions.FindAll("SampleRSI", Symbol, tradeType))
            {
                Print(tradeType);
                ClosePosition(position);
            }
        }

        private void Open(TradeType tradeType)
        {
            var position = Positions.Find("SampleRSI", Symbol, tradeType);

            if (position == null)
                ExecuteMarketOrder(tradeType, Symbol, 1200000, "SampleRSI", 10000, 10000);
        }

        private void manageOpendPositions()
        {
            Position pos = Positions[0];
            if (Account.Balance < Account.Equity && Positions.Count == 1)
            {
                // one winning position, trailing stoploss
                if (Positions[0].TradeType == TradeType.Buy)
                {
                    if (Positions[0].StopLoss < Symbol.Ask - (10 * pipValue))
                    {
                        ModifyPosition(Positions[0], Symbol.Ask - (30 * pipValue), Symbol.Ask + (1000 * pipValue));
                    }
                }
                else
                {
                    if (Positions[0].StopLoss > Symbol.Ask + (30 * pipValue))
                    {
                        ModifyPosition(Positions[0], Symbol.Ask + (30 * pipValue), Symbol.Ask - (1000 * pipValue));
                    }
                }


            }
        }

    }





}






