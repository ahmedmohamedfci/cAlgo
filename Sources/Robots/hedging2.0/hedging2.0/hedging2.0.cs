using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.Indicators;
using cAlgo.API.Indicators;

namespace cAlgo.Robots
{
    [Robot()]
    public class hedging2 : Robot
    {

        #region parameters

        [Parameter("Source")]
        public DataSeries Source { get; set; }

        [Parameter("block minimum", DefaultValue = 15.0, MinValue = 0)]
        public double blockMin { get; set; }

        [Parameter("standard Deviation period", DefaultValue = 20)]
        public int devperiod { get; set; }


        //[Parameter("deviation limit", DefaultValue = 0.002)]
        //public double devlimit { get; set; }

        [Parameter("moving average type")]
        public MovingAverageType MAtype { get; set; }

        [Parameter("block size", DefaultValue = 5)]
        public double bblock { get; set; }
        #endregion

        #region variables

        public StandardDeviation sdev;
        public MovingAverage ma;
        public BollingerBands bbands;
        public double block;
        public double starting;
        public bool closeallbool;
        public double minEquity;

        #endregion


        protected override void OnStop()
        {
            //closeall();
            Print("minimum equity is {0}\n\n", minEquity);
        }

        protected override void OnStart()
        {
            minEquity = Account.Balance;

            sdev = Indicators.StandardDeviation(Source, devperiod, MAtype);
            ma = Indicators.MovingAverage(Source, devperiod, MAtype);
            bbands = Indicators.BollingerBands(Source, devperiod, 2, MAtype);
            Timer.Start(1);


            Positions.Closed += OnPositionClosed;

            setBlock();

        }




        protected override void OnTick()
        {


            if (canIMakeTrade())
            {
                closeallbool = false;
                makeAtrade();
                starting = Account.Balance;

            }


            if (minEquity > Account.Equity)
                minEquity = Account.Equity;


        }

        public bool canIMakeTrade()
        {
            if (Positions.Count > 0)
            {
                Print("count");
                return false;

            }
            //Print("the SD result is: {0}\n", sdev.Result.Last(0));
            //if (sdev.Result.Last(0) < devlimit)
            //    return false;

            if (bbands.Top.Last(0) - bbands.Bottom.Last(0) < block * 2 / 100000)
            {
                Print("top: {0} , buttom:{1}, block: {2}\n", bbands.Top.Last(0), bbands.Bottom.Last(0), block);
                return false;

            }


            if (block <= blockMin)
            {
                Print("block {0} < {1}", block, blockMin);
                return false;

            }

            return true;
        }


        public void closeall()
        {
            foreach (Position p in Positions)
            {
                ClosePosition(p);
            }
        }

        public void OnPositionClosed(PositionClosedEventArgs obj)
        {
            if (Account.Equity >= starting)
            {
                Print("closing positions\n\n");
                closeallbool = true;
                closeall();
                closeallbool = false;
            }

            if (closeallbool == true)
            {
                return;

            }

            if (obj.Position.TradeType == TradeType.Buy)
            {

                ExecuteMarketOrderAsync(TradeType.Buy, Symbol, 10000, "", 9999999, block);
                //Print("orders pending: {0},,, positions: {1} ", PendingOrders.Count, Positions.Count);
            }
            else
            {
                ExecuteMarketOrderAsync(TradeType.Sell, Symbol, 10000, "", 9999999, block);
            }

        }


        public void makeAtrade()
        {
            //Print("the ma result is: {0}---{1}\n", ma.Result.Last(0), ma.Result.Last(1));
            ExecuteMarketOrderAsync(TradeType.Buy, Symbol, 10000, "", 9999999, block);
            ExecuteMarketOrderAsync(TradeType.Sell, Symbol, 10000, "", 9999999, block);

        }


        public void setBlock()
        {
            double mean = 0;

            for (int i = 0; i < 50; i++)
            {
                mean += MarketSeries.High.Last(i) - MarketSeries.Low.Last(i);
            }

            //mean /= 25;
            block = mean;
            block *= 10000;
            block = Math.Ceiling(block);
            block = bblock;
            Print("block size is: {0}", block);
        }



    }
}
