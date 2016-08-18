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
        [Parameter(DefaultValue = 10000)]
        public int volume { get; set; }


        [Parameter(DefaultValue = 3)]
        public double pips { get; set; }

        [Parameter(DefaultValue = 50)]
        public double losingpips { get; set; }

        [Indicator(IsOverlay = false, TimeZone = TimeZones.EasternStandardTime)]

        int[] usaTime = 
        {
            8,
            9,
            10,
            11,
            12,
            13,
            14,
            15,
            16
        };
        int[] londonTime = 
        {
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            10,
            11
        };
        int[] sydnyTime = 
        {
            17,
            18,
            19,
            20,
            21,
            22,
            23,
            24,
            1
        };
        int[] tokyoTime = 
        {
            20,
            21,
            22,
            23,
            24,
            1,
            2,
            3
        };



        private colosi _Ci;

        protected override void OnStart()
        {
            _Ci = Indicators.GetIndicator<colosi>(14, 9, 6, 6, 12, 26);
        }

        protected override void OnTick()
        {

        }

        protected override void OnBar()
        {
            // if this is not the new york (USA) session do nothing.
            if (!doesArrayContain(usaTime, MarketSeries.OpenTime.LastValue.Hour))
                return;
            //Print("1");
            // only 1 position can be opened per sympol
            if (Positions.Count > 0)
                return;
            //Print("2");

            //Print("{0}   {1} {2}", MarketSeries.Open.Maximum(24), MarketSeries.Open.LastValue, (MarketSeries.Open.Maximum(24) >= MarketSeries.Open.LastValue));
            // getting the open and close for the past 24 hours and comparing them with the current open and close
            if (MarketSeries.Open.Maximum(24) <= MarketSeries.Open.LastValue)
            {
                Print("highest in 24 hours");
                return;
            }
            //Print("3");
            if (MarketSeries.Close.Minimum(24) >= MarketSeries.Open.LastValue + 0.001)
            {
                Print("lowest in 24 hours");
                return;
            }


            double result = _Ci.Result.LastValue;
            Print(result);
            if (result > 3)
            {
                ExecuteMarketOrder(TradeType.Buy, Symbol, volume, "Buy", losingpips, pips);
            }
            else if (result < -3)
            {
                ExecuteMarketOrder(TradeType.Sell, Symbol, volume, "sell", losingpips, pips);
            }
//            Print("5");



        }
        /*
            if (doesArrayContain(sydnyTime, MarketSeries.OpenTime.LastValue.Hour))
                Print("sydny");
            if (doesArrayContain(londonTime, MarketSeries.OpenTime.LastValue.Hour))
                Print("london");
            if (doesArrayContain(tokyoTime, MarketSeries.OpenTime.LastValue.Hour))
                Print("tokyo");
                */




        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }

        public bool doesArrayContain(int[] array, int number)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (number == array[i])
                    return true;
            }
            return false;
        }
    }
}
