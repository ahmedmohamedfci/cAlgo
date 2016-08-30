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




        #region parameters
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
        #endregion

        [Output("Main")]

        #region variables and indicators
        public IndicatorDataSeries Result { get; set; }


        private RelativeStrengthIndex rsi;

        private StochasticOscillator stoch;

        private MacdCrossOver macd;

        private DirectionalMovementSystem dmi;
        private WilliamsPctR william;
        private CommodityChannelIndex ccind;
        private UltimateOscillator UO;



        #endregion


        protected override void Initialize()
        {
            rsi = Indicators.RelativeStrengthIndex(MarketSeries.Open, RSIPeriod);
            stoch = Indicators.StochasticOscillator(stoch1, stoch2, stoch3, MovingAverageType.Exponential);
            macd = Indicators.MacdCrossOver(26, 12, 14);

            dmi = Indicators.DirectionalMovementSystem(RSIPeriod);
            william = Indicators.WilliamsPctR(RSIPeriod);
            ccind = Indicators.CommodityChannelIndex(RSIPeriod);

            UO = Indicators.UltimateOscillator(10, 20, 30);
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

            //Print("{0} = {1} = {2} = {3} = {4} = {5}", result, rsiResult(index), stochRsiResult(index), ADXresult(index), williams(index), cciResult(index));


            Result[index] = result;
        }

        private int rsiResult(int index)
        {

            if (rsi.Result[index] >= 60)
            {
                return -1;
            }
            else if (rsi.Result[index] <= 40)
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

            double average = (stoch.PercentD[index] + stoch.PercentK[index]) / 2;

            if (average < 40)
            {
                if (stoch.PercentK[index] > stoch.PercentD[index])
                {
                    return 1;
                }
            }
            else if (average > 60)
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

            //Print("{0} === {1} == {2}", macd.MACD[index], macd.Signal[index], macd.Histogram[index]);

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


        private int ultimateOsciResult(int index)
        {
            if (UO.Result[index] > 60)
            {
                return 1;

            }

            if (UO.Result[index] < 40)
            {
                return -1;
            }
            return 0;
        }



    }
}
