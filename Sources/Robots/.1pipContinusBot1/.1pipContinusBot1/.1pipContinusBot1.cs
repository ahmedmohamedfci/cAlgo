using System;
using System.Linq;
using System.Threading;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using System.Collections.Generic;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class NewcBot : Robot
    {
        [Parameter(DefaultValue = 50.0)]
        public double stopLoss { get; set; }

        [Parameter(DefaultValue = 1.0)]
        public double takeProfit { get; set; }

        [Parameter(DefaultValue = 10000)]
        public int volume { get; set; }

        // this is the number of pips between each trade and the other and between the 
        // next trade and the closest support or risestance
        [Parameter(DefaultValue = 0.002)]
        public double distance { get; set; }

        Random r = new Random();
        int tradeNumber = 0;
        TradeResult ts;
        LinkedList<Position> buyPositions = new LinkedList<Position>();
        LinkedList<Position> sellPositions = new LinkedList<Position>();
        LinkedList<Position> closedBuyPositions = new LinkedList<Position>();
        LinkedList<Position> closedSellPositions = new LinkedList<Position>();

        double lastBuyClosePips;
        double lastSellClosePips;
        double lastBuyOpenPips;
        double lastSellOpenPips;


        protected override void OnStart()
        {
            tradeNumber++;
            ts = ExecuteMarketOrder(TradeType.Buy, Symbol, volume, tradeNumber.ToString(), stopLoss, takeProfit);
            buyPositions.AddFirst(ts.Position);



            tradeNumber++;
            ts = ExecuteMarketOrder(TradeType.Sell, Symbol, volume, tradeNumber.ToString(), stopLoss, takeProfit);
            sellPositions.AddFirst(ts.Position);

            lastSellClosePips = Symbol.Bid;
            lastBuyClosePips = Symbol.Ask;
            lastSellOpenPips = Symbol.Bid;
            lastBuyOpenPips = Symbol.Ask;

            Positions.Closed += Positions_Closed;
        }

        void Positions_Closed(PositionClosedEventArgs obj)
        {
            if (obj.Position.TradeType == TradeType.Sell)
            {
                lastSellClosePips = Symbol.Bid;
                closedSellPositions.AddFirst(obj.Position);
                sellPositions.Remove(obj.Position);
            }
            else
            {
                lastBuyClosePips = Symbol.Ask;
                closedBuyPositions.AddFirst(obj.Position);
                buyPositions.Remove(obj.Position);
            }
        }


        protected override void OnBar()
        {
            //maybe put a logic that closes the deal if more than 3 candles passed??

        }

        protected override void OnTick()
        {
            if (Positions.Count >= 2)
                return;

            if (buyPositions.Count < 1)
            {
                checkBuy();
            }
            if (sellPositions.Count < 1)
            {
                checkSell();
            }
        }

        public void checkBuy()
        {
            // checkBuy last buy close & support
            //Print(lastBuyClosePips <= Symbol.Bid - distance, lastBuyClosePips >= Symbol.Bid + distance, lastBuyClosePips, Symbol.Bid + distance);
            if (Math.Abs(lastBuyOpenPips - Symbol.Ask) <= distance)
            {
                return;
            }

            bool flagPast10 = false;
            for (int i = 0; i < 10; i++)
            {

                //Print(Symbol.Ask >= MarketSeries.Open.Last(i) + distance, MarketSeries.Open.Last(i) + distance, Symbol.Ask);
                if (Math.Abs(Symbol.Ask - MarketSeries.High.Last(i)) >= distance)
                {
                    flagPast10 = true;
                }
            }
            if (flagPast10 == false)
                return;

            // check min distance between the current sell positions



            tradeNumber++;
            ts = ExecuteMarketOrder(TradeType.Buy, Symbol, volume, tradeNumber.ToString(), stopLoss, takeProfit);
            buyPositions.AddFirst(ts.Position);
            lastBuyOpenPips = ts.Position.EntryPrice;

        }
        public void checkSell()
        {

            // checkBuy last sell close & resistence
            //if (lastSellClosePips >= Symbol.Ask - distance && lastBuyClosePips <= Symbol.Ask + distance)
            //{
            //    //    return;

            //}


            if (Math.Abs(lastSellOpenPips - Symbol.Bid) <= distance)
            {

                return;

            }

            bool flagPast10 = false;
            for (int i = 0; i < 10; i++)
            {

                //Print(Symbol.Ask >= MarketSeries.Open.Last(i) + distance, MarketSeries.Open.Last(i) + distance, Symbol.Ask);
                if (Math.Abs(Symbol.Bid - MarketSeries.Low.Last(i)) >= distance)
                {
                    flagPast10 = true;
                }
            }
            if (flagPast10 == false)
                return;

            // check min distance between the current sell positions



            tradeNumber++;
            ts = ExecuteMarketOrder(TradeType.Sell, Symbol, volume, tradeNumber.ToString(), stopLoss, takeProfit);
            sellPositions.AddFirst(ts.Position);
            lastSellOpenPips = ts.Position.EntryPrice;
        }

        protected override void OnStop()
        {
            foreach (Position p in Positions)
                ClosePosition(p);
        }
    }
}
