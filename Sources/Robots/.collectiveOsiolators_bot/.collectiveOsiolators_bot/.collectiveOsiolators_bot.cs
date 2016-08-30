using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class colosiBot : Robot
    {
        #region parameters
        [Parameter(DefaultValue = 14)]
        public int period { get; set; }

        [Parameter(DefaultValue = 9)]
        public int stoch1 { get; set; }

        [Parameter(DefaultValue = 6)]
        public int stoch2 { get; set; }

        [Parameter(DefaultValue = 6)]
        public int stoch3 { get; set; }

        [Parameter(DefaultValue = 100000)]
        public int volume { get; set; }

        [Parameter(DefaultValue = 12)]
        public double macdLow { get; set; }
        [Parameter(DefaultValue = 26)]
        public double macdHeigh { get; set; }

        [Parameter(DefaultValue = 0.0001)]
        public double pipValue { get; set; }
        #endregion parameters


        private colosi _Ci;

        protected override void OnStart()
        {
            _Ci = Indicators.GetIndicator<colosi>(period, stoch1, stoch2, stoch3, macdLow, macdHeigh);
        }

        protected override void OnBar()
        {
            if (Positions.Count > 0)
            {
                manageOpendPositions();

                // checkToClose();
                return;
            }
            else
            {
                if (MarketSeries.OpenTime.LastValue.DayOfWeek.CompareTo(DayOfWeek.Friday) == 0 || MarketSeries.OpenTime.LastValue.DayOfWeek.CompareTo(DayOfWeek.Thursday) == 0)
                {
                    return;
                }
            }



            double result = _Ci.Result.LastValue;
            long vol = volume;
            vol = vol * (long)Math.Abs(result);
            if (result > 3)
            {
                ExecuteMarketOrder(TradeType.Buy, Symbol, vol, "lan", 500, 5);
            }
            else if (result < -3)
            {
                ExecuteMarketOrder(TradeType.Sell, Symbol, vol, "lan", 500, 5);
            }

        }

        protected override void OnStop()
        {
            foreach (Position p in Positions)
            {
                ClosePosition(p);
            }
        }

        private void manageOpendPositions()
        {
            if (MarketSeries.OpenTime.LastValue.DayOfWeek.CompareTo(DayOfWeek.Friday) == 0 && Positions.Count > 0)
            {

                ClosePosition(Positions[0]);
                return;
            }
            Position pos = Positions[0];
            if (Account.Balance < Account.Equity && Positions.Count == 1)
            {
                // one winning position, trailing stoploss

                if (Positions[0].TradeType == TradeType.Buy)
                {

                    if (Positions[0].StopLoss < GetAbsoluteTakeProfit(pos, 30))
                    {
                        ModifyPosition(Positions[0], GetAbsoluteTakeProfit(pos, 10), GetAbsoluteStopLoss(pos, 1000));
                    }
                }
                else
                {

                    if (Positions[0].StopLoss > GetAbsoluteTakeProfit(pos, 30))
                    {
                        ModifyPosition(Positions[0], GetAbsoluteTakeProfit(pos, 10), GetAbsoluteStopLoss(pos, 1000));
                    }
                }


            }
        }

        private void checkToClose()
        {
            if (_Ci.Result.LastValue > 0 && Positions[0].TradeType == TradeType.Sell)
            {
                ClosePosition(Positions[0]);
            }

            if (_Ci.Result.LastValue < 0 && Positions[0].TradeType == TradeType.Buy)
            {
                ClosePosition(Positions[0]);
            }
        }

        private double GetAbsoluteStopLoss(Position position, int stopLossInPips)
        {
            return position.TradeType == TradeType.Buy ? position.EntryPrice - Symbol.PipSize * stopLossInPips : Symbol.Ask + Symbol.PipSize * stopLossInPips;
        }

        private double GetAbsoluteTakeProfit(Position position, int takeProfitInPips)
        {
            return position.TradeType == TradeType.Buy ? position.EntryPrice + Symbol.PipSize * takeProfitInPips : Symbol.Ask - Symbol.PipSize * takeProfitInPips;
        }

    }
}
