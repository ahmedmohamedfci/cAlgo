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
    public class _collectiveIndicators_botneverlose : Robot
    {

        private coolective_indic _Ci;

        [Parameter(DefaultValue = 0.0001)]
        public int pipValue { get; set; }

        [Parameter(DefaultValue = 50000)]
        public int volume { get; set; }


        [Parameter(DefaultValue = 1, MinValue = 1, MaxValue = 4)]
        public int mode { get; set; }

        [Parameter(DefaultValue = 50, MinValue = 50, MaxValue = 100)]
        public int hedgeStartPips { get; set; }

        [Parameter(DefaultValue = 250, MinValue = 200, MaxValue = 400)]
        public int LoseCrossLine { get; set; }

        int lastTrade = 1;

        string name1 = "start trade", name2 = "hedged trade";

        protected override void OnStart()
        {
            _Ci = Indicators.GetIndicator<coolective_indic>(0.0001);
            //Print(_Ci.Result);

            Positions.Closed += new Action<PositionClosedEventArgs>(Positions_Closed);
        }


        protected override void OnStop()
        {
            foreach (Position p in Positions)
            {
                ClosePosition(p);
            }
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
                if (lastTrade == 1)
                {
                    OpenOriginal(TradeType.Buy, name1);
                    lastTrade = 0;
                }
                else if (lastTrade == 0)
                {
                    OpenOriginal(TradeType.Sell, name1);
                    lastTrade = 1;
                }
                else
                {
                    Print(lastTrade);
                }
            }


        }


        private void OpenOriginal(TradeType tradeType, string name)
        {
            var position = Positions.Find(name, Symbol, tradeType);

            if (position == null)
                ExecuteMarketOrder(tradeType, Symbol, volume, name, 10000, 10000);
        }

        private void OpenHedge(TradeType tradeType, string name)
        {
            var position = Positions.Find(name, Symbol, tradeType);


            if (position == null)
            {

                ExecuteMarketOrder(tradeType, Symbol, volume * 2, name, 10000, 10000);
            }
        }

        private void manageOpendPositions()
        {

            #region 2 trades
            if (Positions.Count == 2)
            {
                hedgeManagement();
            }
            #endregion 2trades

            #region one losing trade
            // apply hedging staratigies
            if (Account.Balance > Account.Equity && Positions.Count == 1 && Positions[0].Pips < hedgeStartPips)
            {

                                /* 
                * in hedging we have to test 2 stratigies, 
                * 1- during having trade x losing, open trade y in opposit direction after A pips..
                * and hold till y break even and always keep only A pips of losing
                * but if the X position reached B pips losing or after a specified time, close both trades and start new 
                * 2- during having trade x losing, open trade y in opposit direction if indicators give signal to 
                * the opposite direction and perform the same stop loss and everything... very normal trading in the opposit direction
                * the second way is more risky but would give much more pips
                */


Position pos = Positions[0];

                if (mode == 1)
                {
                    if (Positions[0].TradeType == TradeType.Buy)
                    {

                        hedgeStart(1, TradeType.Sell);
                    }
                    else
                    {
                        hedgeStart(1, TradeType.Buy);
                    }
                }
                else if (mode == 2)
                {
                    if (Positions[0].TradeType == TradeType.Buy)
                    {
                        hedgeStart(2, TradeType.Sell);
                    }
                    else
                    {
                        hedgeStart(2, TradeType.Buy);
                    }

                }
                else if (mode == 3)
                {
                    if (Positions[0].TradeType == TradeType.Buy)
                    {
                        hedgeStart(3, TradeType.Sell);
                    }
                    else
                    {
                        hedgeStart(3, TradeType.Buy);

                    }

                }
                else if (mode == 4)
                {
                    Print((Positions[0].EntryTime));

                    //if (Positions[0].EntryTime.AddDays(3).CompareTo(MarketSeries.OpenTime.LastValue) == -1)
                    if (Positions[0].Pips < -250)
                    {
                        //lastTrade = 1 - lastTrade;
                        ClosePosition(Positions[0]);
                    }
                }


            }
            #endregion end of one losing trade

            #region one winning trade
            else if (Account.Balance < Account.Equity && Positions.Count == 1)
            {

                Position pos = Positions[0];
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
            #endregion end of one winning trade
        }


        private void hedgeStart(int mode, TradeType hedgeDirection)
        {
            if (mode == 1)
            {
                OpenHedge(hedgeDirection, name2);
            }
            if (mode == 2)
            {
                double buy = _Ci.buy.LastValue;
                double sell = _Ci.sell.LastValue;
                double reverse = _Ci.reverse.LastValue;

                if (buy > sell && hedgeDirection == TradeType.Buy)
                {
                    OpenHedge(TradeType.Buy, name2);

                }

                if (buy < sell && hedgeDirection == TradeType.Sell)
                {
                    OpenHedge(TradeType.Sell, name2);

                }


            }
        }


        private void hedgeManagement()
        {
            Position original = Positions[0];
            Position hedged = Positions[1];

            if (mode == 1)
            {
                // check if the hedged trade is winning then put the stoploss on its breakeven
                // if the hedging trade is losing then if it reaches the 
                //first deal close it losing (only if indicators says the other deal will win)
                if (hedged.Pips > 30)
                {
                    if (hedged.TradeType == TradeType.Buy)
                    {

                        ModifyPosition(hedged, hedged.EntryPrice - 30 * pipValue, hedged.EntryPrice + 200 * pipValue);
                    }
                    else
                    {
                        ModifyPosition(hedged, hedged.EntryPrice + 30 * pipValue, hedged.EntryPrice - 200 * pipValue);
                    }

                }

            }
            else if (mode == 2)
            {
                if (hedged.Pips < 0)
                    return;
                if (hedged.TradeType == TradeType.Buy)
                {
                    if (hedged.StopLoss < Symbol.Ask - (10 * pipValue))
                    {
                        ModifyPosition(hedged, Symbol.Ask - (30 * pipValue), Symbol.Ask + (1000 * pipValue));
                    }
                }
                else
                {
                    if (hedged.StopLoss > Symbol.Ask + (30 * pipValue))
                    {
                        ModifyPosition(hedged, Symbol.Ask + (30 * pipValue), Symbol.Ask - (1000 * pipValue));
                    }
                }


            }
        }

        void Positions_Closed(PositionClosedEventArgs obj)
        {
            if (obj.Position.Label == name1)
            {
                OnStop();
            }
        }




    }





}






