using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Requests;
using cAlgo.Indicators;
using cAlgo.API.Indicators;

namespace cAlgo.Robots
{
    [Robot()]
    public class hedging2 : Robot
    {

        #region parameters

/*
		[Parameter("Source")]
		public DataSeries Source { get; set; }

		[Parameter("block minimum", DefaultValue = 15.0, MinValue = 0)]
		public double blockMin { get; set; }

		[Parameter("standard Deviation period", DefaultValue = 20)]
		public int devperiod { get; set; }




		[Parameter("moving average type")]
		public MovingAverageType MAtype { get; set; }

		[Parameter("block size", DefaultValue = 5)]
		public double bblock { get; set; }

		 */
        #endregion
        #region variables
                /*
		public StandardDeviation sdev;
		public MovingAverage ma;
		public BollingerBands bbands;
		public double block;



		 */
        public bool closeallbool;
        public double starting;
        public double minEquity;
        public double stopLose;
        public double takeProfit;
        //public double block;
        public MarketSeries m1;

        [Parameter("block", DefaultValue = 10)]
        public double block { get; set; }

        [Parameter("time frame")]
        public TimeFrame tf { get; set; }
        #endregion

        protected override void OnStop()
        {
            //closeall();
            Print("minimum equity is {0}\n\n", minEquity);



        }

        protected override void OnStart()
        {
            minEquity = Account.Balance;

            stopLose = 5;
            takeProfit = stopLose * 2;
            m1 = MarketData.GetSeries(tf);

            Timer.Start(1);

            Positions.Closed += OnPositionClosed;



        }

        protected override void OnTick()
        {


            makeAtrade(canIMakeTrade());

            starting = Account.Balance;

            if (minEquity > Account.Equity)
                minEquity = Account.Equity;

        }

        public int canIMakeTrade()
        {
            double lastSecondOpen = MarketSeries.Open.Last(2);
            double lastSecondClose = MarketSeries.Close.Last(2);

            double lastOpen = MarketSeries.Open.Last(1);
            double lastClose = MarketSeries.Close.Last(1);

            if (Positions.Count > 0)
                return 0;


            if (Math.Abs(lastClose - lastOpen) < block)
            {
                Print("sa {0}", Math.Abs(lastClose - lastOpen) * 10000);
                return 0;
            }
            if (Math.Abs(lastSecondOpen - lastSecondClose) < block)
            {
                Print("da {0}", Math.Abs(lastSecondOpen - lastSecondClose) * 10000);
                return 0;
            }
            //Print("differenced {0}  --- {1}--- {2}", 0, Math.Abs(lastClose - lastOpen) < block, Math.Abs(lastSecondOpen - lastSecondClose) < block);


            // buy
            if (lastClose > lastOpen && lastSecondOpen > lastSecondClose)
            {
                return 1;
            }
            // buy
            if (lastClose > lastOpen && lastSecondOpen < lastSecondClose)
            {
                return 2;
            }

            // sell
            if (lastClose < lastOpen && lastSecondOpen > lastSecondClose)
            {
                return 3;
            }
            // sell
            if (lastClose < lastOpen && lastSecondOpen < lastSecondClose)
            {
                return 4;
            }

            return 0;
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

        }
        /*
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
  */

        public void makeAtrade(int type)
        {
            if (type == 0)
                return;

            if (type == 1)
            {

                ExecuteMarketOrderAsync(TradeType.Buy, Symbol, 1000, "", stopLose, takeProfit);
            }
            else if (type == 2)
            {

                ExecuteMarketOrderAsync(TradeType.Sell, Symbol, 1000, "", stopLose, takeProfit);
            }
            else if (type == 3)
            {
                ExecuteMarketOrderAsync(TradeType.Buy, Symbol, 1000, "", stopLose, takeProfit);
            }
            else if (type == 4)
            {
                ExecuteMarketOrderAsync(TradeType.Buy, Symbol, 1000, "", stopLose, takeProfit);
            }
        }


    }


}
