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
        private colosi _Co;

        [Parameter(DefaultValue = 14)]
        public int period { get; set; }

        [Parameter(DefaultValue = 9)]
        public int stoch1 { get; set; }

        [Parameter(DefaultValue = 6)]
        public int stoch2 { get; set; }

        [Parameter(DefaultValue = 6)]
        public int stoch3 { get; set; }

        [Parameter(DefaultValue = 1000)]
        public int volume { get; set; }

        [Parameter(DefaultValue = 12)]
        public double macdLow { get; set; }
        [Parameter(DefaultValue = 26)]
        public double macdHeigh { get; set; }

        [Parameter(DefaultValue = 0.0001)]
        public double pipValue { get; set; }

        public double balance;
        public int index = 1;
        protected override void OnStart()
        {
            _Ci = Indicators.GetIndicator<coolective_indic>(0.0001);
            _Co = Indicators.GetIndicator<colosi>(period, stoch1, stoch2, stoch3, macdLow, macdHeigh);
            //Print(_Ci.Result);
            balance = Account.Balance;

            //   Positions.Closed += new Action<PositionClosedEventArgs>(Positions_Closed);
        }

        protected override void OnStop()
        {

            foreach (Position p in Positions)
            {
                //ClosePosition(p);
            }
            Print("");
        }

        protected override void OnBar()
        {
            double buy = _Ci.buy.LastValue;
            double sell = _Ci.sell.LastValue;
            double reverse = _Ci.reverse.LastValue;
            //if (MarketSeries.OpenTime.LastValue.Day == 1 && Positions.Count > 0)
            if (Positions.Count == 1 && Positions[0].NetProfit <= -Account.Balance / 3)
            {
                foreach (Position p in Positions)
                {
                    Print("position {0} closed for exceeding loss", Positions[0].Label);
                    ClosePosition(p);
                }
            }


            if (Positions.Count > 0)
            {
                manageOpendPositions();
            }
            else
            {
                //if (buy > sell)
                if (_Co.Result.LastValue > 1)
                {
                    Open(TradeType.Buy);

                }

                //if (buy < sell)
                if (_Co.Result.LastValue < -1)
                {
                    Open(TradeType.Sell);

                }
            }


        }



        private void Open(TradeType tradeType)
        {
            index++;
            ExecuteMarketOrder(tradeType, Symbol, 40000, index.ToString(), 10000, 10000);
        }

        private void manageOpendPositions()
        {
            Position pos = Positions[0];
            if (Account.Balance < Account.Equity && Positions.Count == 1)
            {
                // one winning position, trailing stoploss
                if (Positions[0].TradeType == TradeType.Buy)
                {
                    if (Positions[0].StopLoss < Symbol.Ask - (10 * pipValue) && Positions[0].Pips > 10)
                    {

                        ModifyPosition(Positions[0], GetAbsoluteTakeProfit(Positions[0], 10), GetAbsoluteStopLoss(Positions[0], 1000));
                    }
                }
                else
                {
                    if (Positions[0].StopLoss > Symbol.Ask + (10 * pipValue) && Positions[0].Pips > 10)
                    {
                        ModifyPosition(Positions[0], Symbol.Ask + (10 * pipValue), Symbol.Ask - (1000 * pipValue));
                    }
                }


            }
        }



        private double GetAbsoluteStopLoss(Position position, int stopLossInPips)
        {
            return position.TradeType == TradeType.Buy ? position.EntryPrice - Symbol.PipSize * stopLossInPips : position.EntryPrice + Symbol.PipSize * stopLossInPips;
        }

        private double GetAbsoluteTakeProfit(Position position, int takeProfitInPips)
        {
            return position.TradeType == TradeType.Buy ? position.EntryPrice + Symbol.PipSize * takeProfitInPips : position.EntryPrice - Symbol.PipSize * takeProfitInPips;
        }

    }





}






