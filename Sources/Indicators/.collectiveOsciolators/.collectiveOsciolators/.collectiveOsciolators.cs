using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class colosi : Indicator
    {





        [Parameter(DefaultValue = 14)]
        public int RSIPeriod { get; set; }

        [Parameter(DefaultValue = 9)]
        public int stoch1 { get; set; }
        [Parameter(DefaultValue = 6)]
        public int stoch2 { get; set; }
        [Parameter(DefaultValue = 6)]
        public int stoch3 { get; set; }

        [Parameter(DefaultValue = 12)]
        public double macdLow { get; set; }
        [Parameter(DefaultValue = 26)]
        public double macdHeigh { get; set; }


        [Output("Main")]
        public IndicatorDataSeries Result { get; set; }


        private RelativeStrengthIndex rsi;

        private StochasticOscillator stoch;

        private MacdCrossOver macd;

        private DirectionalMovementSystem dmi;
        private WilliamsPctR william;
        private CommodityChannelIndex ccind;

        protected override void Initialize()
        {
            rsi = Indicators.RelativeStrengthIndex(MarketSeries.Close, RSIPeriod);
            stoch = Indicators.StochasticOscillator(stoch1, stoch2, stoch3, MovingAverageType.Simple);
            macd = Indicators.MacdCrossOver(26, 12, 14);

            dmi = Indicators.DirectionalMovementSystem(RSIPeriod);
            william = Indicators.WilliamsPctR(RSIPeriod);
            ccind = Indicators.CommodityChannelIndex(RSIPeriod);
        }

        public override void Calculate(int index)
        {
            int result = 0;
            result += rsiResult(index);
            result += stochResult(index);
            result += stochRsiResult(index);
            result += macdResult(index);
            result += ADXresult(index);
            result += williams(index);
            result += cciResult(index);

            Result[index] = result;
        }

        private int rsiResult(int index)
        {

            if (rsi.Result[index] > 70)
            {
                return -1;
            }
            else if (rsi.Result[index] < 30)
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }

        private int stochResult(int index)
        {
            //Print("k={0},d={1}", stoch.PercentK[index], stoch.PercentD[index]);
            //ChartObjects.DrawVerticalLine("vLine1" , index, Colors.Red, 1, LineStyle.Dots);

            if (stoch.PercentD[index] < 20 && stoch.PercentK[index] < 20)
            {
                if (stoch.PercentK[index] > stoch.PercentD[index])
                {
                    return 1;
                }
            }
            else if (stoch.PercentD[index] > 80 && stoch.PercentK[index] > 80)
            {
                if (stoch.PercentK[index] < stoch.PercentD[index])
                {
                    return 1;
                }
            }

            return 0;

        }

        private int stochRsiResult(int index)
        {
            double rsiL = rsi.Result[index];
            double rsiH = rsi.Result[index];
            for (int i = index - RSIPeriod + 1; i <= index; i++)
            {
                if (rsiH < rsi.Result[i])
                {
                    rsiH = rsi.Result[i];
                }
                if (rsiL > rsi.Result[i])
                {
                    rsiL = rsi.Result[i];
                }
            }
            if (rsi.Result[index] != rsiL && rsiH != rsiL)
            {
                if (((rsi.Result[index] - rsiL) / (rsiH - rsiL)) > 0.8)
                {
                    return -1;
                }
                else if (((rsi.Result[index] - rsiL) / (rsiH - rsiL)) < 0.2)
                {
                    return 1;
                }
            }

            return 0;

        }


        private int macdResult(int index)
        {
            int total = 0;

            if (macd.MACD[index] > macd.Signal[index])
            {
                total++;
            }
            if (macd.MACD[index] < macd.Signal[index])
            {
                total--;
            }

            return total;

        }

        private int ADXresult(int index)
        {
            if (dmi.DIPlus[index] > dmi.DIMinus[index])
            {
                if (dmi.ADX[index] > dmi.ADX[index - RSIPeriod])
                {
                    return 1;
                }
            }
            if (dmi.DIPlus[index] < dmi.DIMinus[index])
            {
                if (dmi.ADX[index] < dmi.ADX[index - RSIPeriod])
                {
                    return -1;
                }
            }

            return 0;
        }

        private int williams(int index)
        {
            if (william.Result[index] > -80)
            {

                if (william.Result[index - RSIPeriod] < -80)
                {
                    return 1;
                }
            }

            if (william.Result[index] < -20)
            {
                if (william.Result[index - RSIPeriod] > -20)
                {
                    return -1;
                }
            }
            return 0;
        }


        private int cciResult(int index)
        {
            if (ccind.Result[index] > -100)
            {

                if (ccind.Result[index - RSIPeriod] <= -100)
                {

                    return 1;
                }
            }

            if (ccind.Result[index] < 100)
            {
                if (ccind.Result[index - RSIPeriod] > 100)
                {
                    return -1;
                }
            }
            return 0;
        }



    }
}
