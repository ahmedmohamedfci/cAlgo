using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class coolective_indicneverlosing : Indicator
    {
        #region parameters
        [Parameter(DefaultValue = 0.0001)]
        public double pipValue { get; set; }

        public int ret = 0;

        /*
        [Parameter("Data Source")]
        public DataSeries MarketSeries.Close { get; set; }
        */




        [Output("buy", Color = Colors.OrangeRed)]
        public IndicatorDataSeries buy { get; set; }

        [Output("sell", Color = Colors.Green)]
        public IndicatorDataSeries sell { get; set; }

        [Output("trend reverse", Color = Colors.Green)]
        public IndicatorDataSeries reverse { get; set; }


        protected override void Initialize()
        {

            // Initialize and create nested indicators
        }
        #endregion

        public override void Calculate(int index)
        {

            buy[index] = 0;
            sell[index] = 0;
            reverse[index] = 0;
            if (RSI(index) == 1)
                buy[index]++;
            else if (RSI(index) == -1)
                sell[index]++;

            if (doji(index, pipValue, 1, 5, 5) == 1)
                reverse[index]++;


            if (hammer(index) == 1)
                reverse[index]++;


            if (hangingMan(index) == 1)
                reverse[index]++;
            if (InvertedHammer(index) == 1)
            {
                reverse[index]++;
                sell[index]++;
            }

            if (shootingStar(index) == 1)
            {
                sell[index]++;
                reverse[index]++;
            }

            if (WhiteMarubozu(index) == 1)
                buy[index]++;
            if (BlackMarubozu(index) == 1)
                sell[index]++;

            if (trendCrossoverDown(55, 21, index, MarketSeries.Close, pipValue) == 1 && trendCrossoverEnterConfirmation(55, 21, index, MarketSeries.Close, pipValue) == 1)
            {
                sell[index]++;
            }

            if (trendCrossoverUp(55, 21, index, MarketSeries.Close, pipValue) == 1 && trendCrossoverEnterConfirmation(55, 21, index, MarketSeries.Close, pipValue) == 1)
            {
                buy[index]++;
            }

            if (bullishEngulfing(index) == 1)
                buy[index]++;
            if (bearishEngulfing(index) == 1)
                sell[index]++;


        }

        /*
            if (TweezerTops(index - 1, MarketSeries.Close) == 1)
            {
                ChartObjects.DrawVerticalLine("TweezerTops" + index.ToString(), index, Colors.Red, 1, LineStyle.Dots);
            }

            if (doji(index - 1, pipValue, 1, 5, 5) == 1)
            {
                ChartObjects.DrawVerticalLine("vLine" + index.ToString(), index, Colors.Red, 1, LineStyle.Dots);
            }

            if (hangingMan(index - 1) == 1)
            {
                ChartObjects.DrawVerticalLine("hangingMan" + index.ToString(), index, Colors.Red, 1, LineStyle.Dots);
            }
            if (hammer(index - 1) == 1)
            {
                ChartObjects.DrawVerticalLine("hammer" + index.ToString(), index, Colors.Blue, 1, LineStyle.Dots);
            }

            if (trendCrossoverDown(55, 21, index, MarketSeries.Close, pipValue) == 1)
            {
                ChartObjects.DrawLine("trendDown" + index.ToString(), index, MarketSeries.High[index], index, MarketSeries.Low[index], Colors.Blue, 2, LineStyle.Solid);
            }

            if (trendCrossoverUp(55, 21, index, MarketSeries.Close, pipValue) == 1)
            {
                ChartObjects.DrawLine("trendUp" + index.ToString(), index, MarketSeries.High[index], index, MarketSeries.Low[index], Colors.Red, 2, LineStyle.Dots);
            }

            if (shootingStar(index - 1) == 1)
            {
                ChartObjects.DrawVerticalLine("shootingStar" + index.ToString(), index, Colors.Blue, 1, LineStyle.Dots);
            }

            if (InvertedHammer(index - 1) == 1)
            {
                ChartObjects.DrawVerticalLine("invertedhammer" + index.ToString(), index, Colors.Red, 1, LineStyle.Dots);
            }

            if (bullishEngulfing(index - 1) == 1)
            {
                ChartObjects.DrawVerticalLine("bullish englufing" + index.ToString(), index, Colors.Blue, 1, LineStyle.Dots);
            }

            if (bearishEngulfing(index - 1) == 1)
            {
                ChartObjects.DrawVerticalLine("bearish englufing" + index.ToString(), index, Colors.Red, 1, LineStyle.Dots);
            }
                       
        */



        #region finished functions

        #region single candle stick pattern
        public int doji(int index, double pipValue = 0.0001, double maxBody = 1, double minUp = 5, double minDown = 5)
        {
            // nuteral... may reverse the trend
            // have 5 types... may reverse trend

            maxBody = maxBody * pipValue;
            minUp = minUp * pipValue;
            minDown = minDown * pipValue;

            double diff = MarketSeries.High[index] - MarketSeries.Open[index];
            if (Math.Abs(MarketSeries.Open[index] - MarketSeries.Close[index]) <= maxBody)
            {


                if (Math.Abs(MarketSeries.High[index] - MarketSeries.Open[index]) >= minUp)
                {

                    if (MarketSeries.Open[index] - MarketSeries.Low[index] >= minDown)
                    {

                        return 1;
                    }
                }
            }

            return 0;
        }

        public int hammer(int index, double pipValue = 0.0001, double maxBody = 5, double maxUp = 5, double minDown = 15)
        {
            // nuteral... may reverse the trend


            if (maxBody < 2)
                maxBody = 2;


            double diff = MarketSeries.Open[index] - MarketSeries.Close[index];
            diff /= pipValue;

            if ((int)diff <= 0)
                return 0;


            if (diff <= maxBody)
            {
                maxBody = diff;
                maxUp = maxBody;
                minDown = maxBody * 3;

                if ((MarketSeries.High[index] - MarketSeries.Open[index]) / pipValue <= maxUp)
                {


                    if ((MarketSeries.Close[index] - MarketSeries.Low[index]) / pipValue >= minDown)
                    {

                        return 1;


                    }
                }
            }

            return 0;
        }

        public int hangingMan(int index, double pipValue = 0.0001, double maxBody = 5, double maxUp = 5, double minDown = 15)
        {
            // nuteral... may reverse the trend


            if (maxBody < 2)
                maxBody = 2;

            double diff = MarketSeries.Open[index] - MarketSeries.Close[index];
            diff /= pipValue;

            if ((int)diff >= 0)
                return 0;
            diff = diff * -1;


            if (diff <= maxBody)
            {
                maxBody = diff;
                maxUp = maxBody;
                minDown = maxBody * 3;

                if ((MarketSeries.High[index] - MarketSeries.Open[index]) / pipValue <= maxUp)
                {

                    if ((MarketSeries.Close[index] - MarketSeries.Low[index]) / pipValue >= minDown)
                    {

                        return 1;

                    }
                }
            }

            return 0;
        }

        public int InvertedHammer(int index, double pipValue = 0.0001, double maxBody = 5, double MinUp = 15, double MaxDown = 5)
        {
            // bearish reversal signal


            if (maxBody < 2)
                maxBody = 2;


            double diff = MarketSeries.Open[index] - MarketSeries.Close[index];
            diff /= pipValue;

            if ((int)diff <= 0)
                return 0;


            if (diff <= maxBody)
            {
                maxBody = diff;
                MaxDown = maxBody;
                MinUp = maxBody * 3;

                if ((MarketSeries.Close[index] - MarketSeries.Low[index]) / pipValue <= MaxDown)
                {


                    if ((MarketSeries.High[index] - MarketSeries.Open[index]) / pipValue >= MinUp)
                    {

                        return 1;


                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// inverted hanging man
        /// bearish reversal signal
        /// </summary>

        public int shootingStar(int index, double pipValue = 0.0001, double maxBody = 5, double MinUp = 15, double MaxDown = 5)
        {
            // nuteral... may reverse the trend


            if (maxBody < 2)
                maxBody = 2;

            double diff = MarketSeries.Open[index] - MarketSeries.Close[index];
            diff /= pipValue;

            if ((int)diff >= 0)
                return 0;
            diff = diff * -1;


            if (diff <= maxBody)
            {
                maxBody = diff;
                MinUp = maxBody * 3;
                MaxDown = maxBody;

                if ((MarketSeries.High[index] - MarketSeries.Close[index]) / pipValue >= MinUp)
                {


                    if ((MarketSeries.Open[index] - MarketSeries.Low[index]) / pipValue <= MaxDown)
                    {

                        return 1;

                    }
                }
            }

            return 0;
        }


        public int WhiteMarubozu(int index, double pipValue = 0.0001, double minBody = 5)
        {
            // bullish... trend go up, buy

            double high, low, open, close;
            high = Math.Ceiling(MarketSeries.High[index] / pipValue);
            close = Math.Ceiling(MarketSeries.Close[index] / pipValue);
            open = Math.Ceiling(MarketSeries.Open[index] / pipValue);
            low = Math.Ceiling(MarketSeries.Low[index] / pipValue);

            if (close - open < minBody)
                return 0;

            if (high == close)
                if (open == low)
                    return 1;

            return 0;
        }


        public int BlackMarubozu(int index, double pipValue = 0.0001, double minBody = 5)
        {
            // bearish... trend go down, sell


            double high, low, open, close;
            high = Math.Ceiling(MarketSeries.High[index] / pipValue);
            close = Math.Ceiling(MarketSeries.Close[index] / pipValue);
            open = Math.Ceiling(MarketSeries.Open[index] / pipValue);
            low = Math.Ceiling(MarketSeries.Low[index] / pipValue);
            if (open - close < minBody)
                return 0;

            if (high == open)
                if (close == low)
                    return 1;

            return 0;
        }

        #endregion single candle stick pattern

        #region trend signals
        public int trendCrossoverDown(int slow, int fast, int index, DataSeries source, double pipvalue)
        {

            // sell signal.. down trend

            ExponentialMovingAverage slowEMA, fastEMA;
            if (slow == 0 || fast == 0)
            {
                slow = 55;
                fast = 21;
            }
            slowEMA = Indicators.ExponentialMovingAverage(source, slow);
            fastEMA = Indicators.ExponentialMovingAverage(source, fast);

            // check if the fast is below the fast



            if ((slowEMA.Result[index] - fastEMA.Result[index]) / pipValue < 1)
            {
                return 0;
            }
            return 1;
        }


        public int trendCrossoverUp(int slow, int fast, int index, DataSeries source, double pipvalue)
        {

            // sell signal.. down trend

            ExponentialMovingAverage slowEMA, fastEMA;
            if (slow == 0 || fast == 0)
            {
                slow = 55;
                fast = 21;
            }
            slowEMA = Indicators.ExponentialMovingAverage(source, slow);
            fastEMA = Indicators.ExponentialMovingAverage(source, fast);

            // check if the fast is below the fast



            if ((fastEMA.Result[index] - slowEMA.Result[index]) / pipValue < 1)
            {
                return 0;
            }
            return 1;
        }

        public int trendCrossoverEnterConfirmation(int slow, int fast, int index, DataSeries source, double pipvalue)
        {

            ExponentialMovingAverage slowEMA, fastEMA;
            if (slow == 0 || fast == 0)
            {
                slow = 55;
                fast = 21;
            }
            slowEMA = Indicators.ExponentialMovingAverage(MarketSeries.Close, slow);
            fastEMA = Indicators.ExponentialMovingAverage(MarketSeries.Close, fast);

            if (Math.Abs(fastEMA.Result[index] - slowEMA.Result[index]) / pipvalue < 9)
                return 0;


            if (trendCrossoverDown(slow, fast, index, source, pipvalue) == 1)
            {
                if (MarketSeries.High[index] < fastEMA.Result[index] && MarketSeries.Low[index] < fastEMA.Result[index] && MarketSeries.High[index] < slowEMA.Result[index] && MarketSeries.Low[index] < slowEMA.Result[index])
                {
                    return 1;
                }
            }
            else if (trendCrossoverUp(slow, fast, index, source, pipvalue) == 1)
            {
                if (MarketSeries.High[index] > fastEMA.Result[index] && MarketSeries.Low[index] > fastEMA.Result[index] && MarketSeries.High[index] > slowEMA.Result[index] && MarketSeries.Low[index] > slowEMA.Result[index])
                {
                    return 1;
                }
            }

            return 0;
        }

        public int trendCrossoverExitSignal(int slow, int fast, int index, DataSeries source, double pipvalue)
        {
            ExponentialMovingAverage slowEMA, fastEMA;
            if (slow == 0 || fast == 0)
            {
                slow = 55;
                fast = 21;
            }
            slowEMA = Indicators.ExponentialMovingAverage(source, slow);
            fastEMA = Indicators.ExponentialMovingAverage(source, fast);

            if (trendCrossoverDown(slow, fast, index, source, pipvalue) == 1)
            {
                if (MarketSeries.High[index] > fastEMA.Result[index] || MarketSeries.Low[index] > fastEMA.Result[index] || (MarketSeries.High[index] > fastEMA.Result[index] || MarketSeries.Low[index] > fastEMA.Result[index]))
                {


                    return 1;

                }
            }
            else if (trendCrossoverUp(slow, fast, index, source, pipvalue) == 1)
            {
                if (MarketSeries.High[index] < fastEMA.Result[index] || MarketSeries.Low[index] < fastEMA.Result[index] || MarketSeries.High[index] < slowEMA.Result[index] || MarketSeries.Low[index] < slowEMA.Result[index])
                {

                    return 1;

                }
            }
            return 0;
            return 0;
        }

        #endregion terend signal

        #region dual candlestick pattern
        /// <summary>
        /// bullish, buy, uptrend
        /// </summary>

        public int bullishEngulfing(int index, double pipValue = 0.0001)
        {
            // bullish, buy

            double body1 = MarketSeries.Open[index - 1] - MarketSeries.Close[index - 1];
            body1 /= pipValue;
            if (body1 <= 2)
                return 0;

            double body2 = MarketSeries.Close[index] - MarketSeries.Open[index];
            body2 /= pipValue;
            if (body2 <= 2)
                return 0;

            if (body2 <= body1 + 2)
                return 0;

            return 1;
        }

        /// <summary>
        /// bearish, sell, downtrend
        /// </summary>

        public int bearishEngulfing(int index, double pipValue = 0.0001)
        {
            // bearish, sell

            double body1 = MarketSeries.Close[index - 1] - MarketSeries.Open[index - 1];
            body1 /= pipValue;
            if (body1 <= 2)
                return 0;

            double body2 = MarketSeries.Open[index] - MarketSeries.Close[index];
            body2 /= pipValue;
            if (body2 <= 2)
                return 0;

            if (body2 <= body1 + 2)
                return 0;

            return 1;
        }
        #endregion


        #region rsi functions
        /// <summary>
        /// return 1 if buy -1 if sell 0 otherwise
        /// </summary>
        /// <returns></returns>
        public int RSI(int index)
        {
            RelativeStrengthIndex rsi = Indicators.RelativeStrengthIndex(MarketSeries.Close, 14);
            if (rsi.Result[index] > 70)
            {
                return 1;
            }
            else if (rsi.Result[index] < 30)
            {
                return 0;
            }

            return 0;

        }


        #endregion

        #endregion finished coding



        public int hangingMan2(int index, double pipValue = 0.0001, double maxBody = 5, double maxUp = 5, double minDown = 15)
        {
            // nuteral... may reverse the trend

            return 0;
        }



        #region needs fixing
        /// <summary>
        /// The tweezers are dual candlestick reversal patterns. This type of candlestick pattern are usually be spotted after an extended uptrend or downtrend, indicating that a reversal will soon occur.
        /// tops is bearish, sell, downtrend
        /// </summary>
        /// <param name="MA"> to check the trend againest the tweezers</param>

        public int TweezerTops(int index, DataSeries source, double pipValue = 0.0001, int MAperiod = 21)
        {


            ExponentialMovingAverage MA = Indicators.ExponentialMovingAverage(source, MAperiod);
            // if (MA.Result[index - 1] - MA.Result[index] >= 1 * pipValue)
            //     return 0;

            double body1 = (MarketSeries.Close[index - 1] - MarketSeries.Open[index - 1]) / pipValue;
            body1 = Math.Ceiling(body1);
            if (body1 <= 2)
                return 0;

            double body2 = (MarketSeries.Open[index] - MarketSeries.Close[index]) / pipValue;
            body2 = Math.Ceiling(body2);
            if (body2 <= 2)
                return 0;
            double top1 = MarketSeries.High[index - 1] / pipValue;
            double top2 = MarketSeries.High[index] / pipValue;
            top1 = Math.Ceiling(top1);
            top2 = Math.Ceiling(top2);
            /*
            if (top1 == MarketSeries.Close[index - 1])
                return 0;

            if (top2 == MarketSeries.Open[index])
                return 0;
            */
            if (body1 != body2)
                return 0;

            if (top1 != top2)
                return 0;

            return 1;
        }

        #endregion






    }
}
