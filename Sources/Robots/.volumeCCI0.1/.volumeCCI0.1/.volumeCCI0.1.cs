using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class NewcBot : Robot
    {
        [Parameter(DefaultValue = 0.0)]
        public double Parameter { get; set; }
        public CommodityChannelIndex cci;
        public VolumeROC volume;

        protected override void OnStart()
        {
            cci = Indicators.CommodityChannelIndex(20);
            volume = Indicators.VolumeROC(14);
        }

        protected override void OnTick()
        {

        }

        protected override void OnBar()
        {
            bool o = openTrade();
            bool c = false, m;
            if (!o)
                c = closeTrade();
            if (!o && !c)
                marting();

        }

        protected override void OnStop()
        {
            if (Positions.Count > 0)
                ClosePosition(Positions[0]);
        }


        private bool openTrade()
        {

            Print("open open");
            // this says the volume is grren or red
            bool greenOrBlueLast = false;
            bool greenOrBlueBeforeLast = false;
            if (Positions.Count > 0)
            {
                return false;
            }
            if (cci.Result.LastValue > -100 && cci.Result.LastValue < 100)
            {
                // no trades, close the function
                return false;
            }

            // check for green or red

            if (volume.Result.Last(1) < volume.Result.Last(2))
            {
                greenOrBlueLast = true;
            }
            if (volume.Result.Last(2) > volume.Result.Last(3))
            {
                greenOrBlueBeforeLast = true;
            }

            if (!greenOrBlueBeforeLast || !greenOrBlueLast)
            {
                return false;
            }

            if (cci.Result.LastValue > 100)
            {
                ExecuteMarketOrder(TradeType.Sell, Symbol, 10000);
            }
            else
            {
                ExecuteMarketOrder(TradeType.Buy, Symbol, 10000);
            }

            Print("open close");
            return true;

        }

        private bool closeTrade()
        {
            Print("open close");
            if (Positions.Count == 0 || Positions[0].NetProfit < 0)
            {
                Print("close close");
                return false;
            }

            if (volume.Result.Last(1) > volume.Result.Last(2))
            {
                ClosePosition(Positions[0]);
            }
            Print("close close");
            return true;
        }

        private bool marting()
        {
            Print("open martinage");
            if (Positions.Count == 0 || Positions[0].NetProfit > 0)
            {
                Print("close marting");
                return false;
            }

            if (volume.Result.Last(1) > volume.Result.Last(2))
            {
                ClosePosition(Positions[0]);
                return false;
            }

            long vol = Positions[0].Volume;
            TradeType tr = Positions[0].TradeType;
            ClosePosition(Positions[0]);
            ExecuteMarketOrder(tr, Symbol, vol * 2);
            Print("close marting");
            return true;
        }
    }
}
