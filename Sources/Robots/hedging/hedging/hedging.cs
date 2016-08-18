using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.Indicators;
using cAlgo.API.Indicators;

namespace cAlgo.Robots
{
// test in GBp/ NZD
    [Robot()]
    public class hedging : Robot
    {

        #region parameters

        [Parameter("Source")]
        public DataSeries Source { get; set; }

        [Parameter("block minimum", DefaultValue = 15.0, MinValue = 0)]
        public double blockMin { get; set; }

        [Parameter("standard Deviation period", DefaultValue = 5)]
        public int devperiod { get; set; }


        //[Parameter("deviation limit", DefaultValue = 0.002)]
        //public double devlimit { get; set; }

        [Parameter("moving average type")]
        public MovingAverageType MAtype { get; set; }


        #endregion

        #region variables

        public StandardDeviation sdev;
        public MovingAverage ma;
        public BollingerBands bbands;
        public double block;

        #endregion



        protected override void OnStart()
        {

            sdev = Indicators.StandardDeviation(Source, devperiod, MAtype);
            ma = Indicators.MovingAverage(Source, devperiod, MAtype);
            bbands = Indicators.BollingerBands(Source, devperiod, 2, MAtype);
            Timer.Start(1);


            Positions.Opened += OnPositionsOpened;
            Positions.Closed += OnPositionClosed;

            setBlock();

        }

        void OnPositionsOpened(PositionOpenedEventArgs obj)
        {
            // this if the position was a pending order
            if (Positions.Count > 1)
            {
                ModifyPositionAsync(Positions[0], obj.Position.TakeProfit, Positions[0].EntryPrice);
                ModifyPositionAsync(Positions[1], Positions[0].EntryPrice, obj.Position.TakeProfit);
                Print("position {0} modified", Positions[0].Id);

            }

        }

        protected override void OnTick()
        {
            setBlock();

            if (canIMakeTrade())
            {

                makeAtrade();

            }

        }

        public bool canIMakeTrade()
        {
            if (Positions.Count > 0)
                return false;

            ///Print("the SD result is: {0}\n", sdev.Result.Last(0));
            //if (sdev.Result.Last(0) < devlimit)
            //    return false;



            if (bbands.Top.Last(0) - bbands.Bottom.Last(0) < block * 2 / 10000)
                return false;


            if (block <= blockMin)
                return false;

            return true;
        }




        public void OnPositionClosed(PositionClosedEventArgs obj)
        {


                        /*
            // if i had a position and a filled order, the position closed also close the pending order
            if (Positions.Count == 1 && PendingOrders.Count == 0)
            {
                ClosePositionAsync(Positions[0]);
            }
*/
if (Positions.Count == 0 && PendingOrders.Count > 0)
            {
                CancelPendingOrderAsync(PendingOrders[0]);

                //Print("orders pending: {0},,, positions: {1} ", PendingOrders.Count, Positions.Count);
            }

        }


        public void makeAtrade()
        {
            //Print("the ma result is: {0}---{1}\n", ma.Result.Last(0), ma.Result.Last(1));
            if (ma.Result.Last(0) > ma.Result.Last(1))
            {
                sell();
            }
            else if (ma.Result.Last(0) < ma.Result.Last(1))
            {
                buy();
            }
        }

        public void buy()
        {
            //Print("bid price is: {0}\n", Symbol.Bid);
            //Print("entery price is: {0}\n", Symbol.Bid - block / 10000);

            PlaceStopOrderAsync(TradeType.Sell, Symbol, 30000, Symbol.Bid - block / 10000, "second sell", block, block);
            ExecuteMarketOrderAsync(TradeType.Buy, Symbol, 10000, "first buy", 2 * block, block);


        }

        public void sell()
        {
            //Print("bid price is: {0}\n", Symbol.Bid);
            //Print("entery price is: {0}\n", Symbol.Bid + block / 10000);
            PlaceStopOrderAsync(TradeType.Buy, Symbol, 30000, Symbol.Bid + block / 10000, "second buy", block, block);
            ExecuteMarketOrderAsync(TradeType.Sell, Symbol, 10000, "first sell", 2 * block, block);

        }


        public void setBlock()
        {
            double mean = 0;

            for (int i = 0; i < 50; i++)
            {
                mean += MarketSeries.High.Last(i) - MarketSeries.Low.Last(i);
            }

            mean /= 25;
            block = mean;
            block *= 10000;
            block = Math.Ceiling(block);
            //block = 15;
            Print("block size is: {0}", block);
        }

    }
}
