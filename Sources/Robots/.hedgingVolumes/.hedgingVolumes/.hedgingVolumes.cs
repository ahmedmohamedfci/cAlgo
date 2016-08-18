////#reference: ..\Indicators\coolective_indic.algo
using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using System.Threading;
using System.Collections.Generic;


/*

close the deal once the candle hits the trend line not on reversal and add trailing stop loss
open the deal only when the 2 trends are going upwards not sideways


*/
namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class _collectiveIndicators_botneverlose : Robot
    {
        #region variables
        private coolective_indic _Ci;

        [Parameter(DefaultValue = 0.0001)]
        public double pipValue { get; set; }

        [Parameter(DefaultValue = 1000)]
        public int volume { get; set; }

        [Parameter(DefaultValue = 50)]
        public int target { get; set; }



        int enough = 0;
        static volatile int locker = 1;
        object o;
        int i = 0;
        LinkedList<Position> mandatoryClosedPositions = new LinkedList<Position>();
        double minEquityDown = 0;
        #endregion variables



        protected override void OnStart()
        {
            Print("bot started");
            _Ci = Indicators.GetIndicator<coolective_indic>(0.0001);

            o = new object();
            Positions.Closed += OnPositionClosed;
            Positions.Opened += OnPositionOpened;

            double buy = _Ci.buy.LastValue;
            double sell = _Ci.sell.LastValue;
            double reverse = _Ci.reverse.LastValue;

            i = 0;

            string lable = Time.Ticks.ToString();
            if (buy > sell)
            {

                Print("+buy operation {1} with 1k on {0}", lable, i);
                ExecuteMarketOrder(TradeType.Buy, Symbol, volume, i.ToString(), 1000, target);
                Print("-pending sell with 3k on {0}", lable);
                //Positions[0].EntryPrice - target * pipValue
                PlaceLimitOrder(TradeType.Sell, Symbol, volume * 3, GetAbsoluteStopLoss(Positions[0], target), (i + 1).ToString(), 1000, target, null);
            }
            //if (buy < sell)
            else
            {

                Print("-sell operation {1} with 1k on {0}", lable, i);
                ExecuteMarketOrder(TradeType.Sell, Symbol, volume, i.ToString(), 1000, target);
                Print("+pending buy with 3k on {0}", lable);
                PlaceStopOrder(TradeType.Buy, Symbol, volume * 3, GetAbsoluteStopLoss(Positions[0], target), (i + 1).ToString(), 1000, target, null);

            }

            Print("on start out");
        }

        public void OnPositionClosed(PositionClosedEventArgs obj)
        {
            i++;
            enough = 0;
            Print("onclose entered on price {0} by object {1}", Symbol.Ask, obj.Position.Label);
            if (mandatoryClosedPositions.Find(obj.Position) != null)
            {
                mandatoryClosedPositions.Remove(obj.Position);
                return;
            }

            foreach (PendingOrder p in PendingOrders)
            {
                Print("canceled pending order{0} volume {1}  ", p.Label, p.Volume);
                CancelPendingOrder(p);

            }

            foreach (Position p in Positions)
            {
                Print("close positio {0} volume {1}  ", p.Label, p.Volume);
                ClosePosition(p);
                mandatoryClosedPositions.AddLast(p);
            }

            Thread.Sleep(10);
            string lable = Time.Ticks.ToString();
            if (Positions.Count == 0)
            {
                if (obj.Position.TradeType == TradeType.Buy)
                {

                    Print("-sell operation {1} with 1k on {0}", lable, i);
                    ExecuteMarketOrder(TradeType.Sell, Symbol, volume, i.ToString(), 1000, target);
                    Print("+pending buy with 3k on {0}", lable);
                    PlaceLimitOrder(TradeType.Buy, Symbol, volume * 3, GetAbsoluteStopLoss(Positions[0], target), (i + 1).ToString(), 1000, target, null);

                }
                else
                {

                    Print("+buy operation {1} with 1k on {0}", lable, i);
                    ExecuteMarketOrder(TradeType.Buy, Symbol, volume, i.ToString(), 1000, target);
                    Print("-pending sell with 3k on {0}", lable);
                    PlaceStopOrder(TradeType.Sell, Symbol, volume * 3, GetAbsoluteStopLoss(Positions[0], target), (i + 1).ToString(), 1000, target, null);

                }
            }
            else
            {
                Print("+++++positions was not totally closed+++++");
            }

            Print("leaving on position closed");
        }



        public void OnPositionOpened(PositionOpenedEventArgs obj)
        {
            i++;


            if (enough == 1)
                return;

            Print("on position opened");
            if (Positions.Count > 1)
            {
                foreach (Position p in Positions)
                {
                    if (p != obj.Position)
                    {
                        ModifyPosition(p, GetAbsoluteStopLoss(p, 1000), GetAbsoluteTakeProfit(p, 1000));
                        Print("position {0} modified, price on {1} take profit on {2} stoploss on {3}", p.Label, Symbol.Ask, p.TakeProfit, p.StopLoss);
                    }
                    else
                    {
                        Print("position {} was the one opened", p.Label);
                    }
                }

                if (obj.Position.TradeType == TradeType.Buy)
                {
                    //target * 2
                    PlaceStopOrder(TradeType.Sell, Symbol, obj.Position.Volume * 2, GetAbsoluteStopLoss(obj.Position, target), (i + 1).ToString(), 1000, target, null);
                    Print("pending sell order {0} modified, price on {1} take profit on {2} stoploss on {3}", PendingOrders[0].Label, Symbol.Ask, PendingOrders[0].TakeProfit, PendingOrders[0].StopLoss);
                    Print("pending order count {0}", PendingOrders.Count);
                }
                else
                {
                    PlaceStopOrder(TradeType.Buy, Symbol, obj.Position.Volume * 2, GetAbsoluteStopLoss(obj.Position, target), (i + 1).ToString(), 1000, target, null);
                    Print("pending buy order {0} modified, price on {1} take profit on {2} stoploss on {3}", PendingOrders[0].Label, Symbol.Ask, PendingOrders[0].TakeProfit, PendingOrders[0].StopLoss);
                    Print("pending order count {0}", PendingOrders.Count);

                }

            }
            else
            {
                Print("only one position should be opened by this time");
            }


            if (Positions.Count >= 10)
                enough = 1;

            Print("on position open going out");
        }



        protected override void OnStop()
        {
            foreach (Position p in Positions)
            {
                Print(p.EntryTime.Minute);
                ClosePosition(p);
            }

            foreach (PendingOrder pen in PendingOrders)
            {
                CancelPendingOrder(pen);
            }


        }

        protected override void OnTick()
        {
            if (minEquityDown > (Account.Balance - Account.Equity))
            {
                minEquityDown = (Account.Balance - Account.Equity);
            }
            if (PendingOrders.Count > 1)
            {
                Print("{0} pending orders", PendingOrders.Count);
                Stop();
            }
            if (enough == 1)
                manageOpendPositions();

            Print("***************{0}*************", minEquityDown);
        }


        private double GetAbsoluteStopLoss(Position position, int stopLossInPips)
        {
            return position.TradeType == TradeType.Buy ? position.EntryPrice - Symbol.PipSize * stopLossInPips : position.EntryPrice + Symbol.PipSize * stopLossInPips;
        }

        private double GetAbsoluteTakeProfit(Position position, int takeProfitInPips)
        {
            return position.TradeType == TradeType.Buy ? position.EntryPrice + Symbol.PipSize * takeProfitInPips : position.EntryPrice - Symbol.PipSize * takeProfitInPips;
        }


        private void manageOpendPositions()
        {

            #region one winning trade
            if (Account.Balance < Account.Equity)
            {

                Position pos = Positions.Last();
                // one winning position, trailing stoploss
                if (Positions.Last().TradeType == TradeType.Buy)
                {
                    if (Positions.Last().StopLoss < Symbol.Ask - (10 * pipValue))
                    {
                        ModifyPosition(Positions.Last(), Symbol.Ask - (30 * pipValue), Symbol.Ask + (1000 * pipValue));
                    }
                }
                else
                {
                    if (Positions.Last().StopLoss > Symbol.Ask + (30 * pipValue))
                    {
                        ModifyPosition(Positions.Last(), Symbol.Ask + (30 * pipValue), Symbol.Ask - (1000 * pipValue));
                    }
                }


            }
            #endregion end of one winning trade
        }



    }





}






