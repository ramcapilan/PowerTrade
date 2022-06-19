using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Configuration;
using System.Threading;

namespace PowerTraderVolume
{
    internal class PowerService
    {
        private Logger logger;
        private double[] tradeData;
        private IEnumerable<Services.PowerTrade> trades;

        public PowerService()
        {
            logger = new Logger();
        }

        public void GetData()
        {
            double[] outputArray = new double[24];
            TryPowerServiceCall(PowerServiceCall);
            var exceptions = new List<Exception>();
            foreach (var trade in trades)
            {
                for (int i = 0; i < 24; i++)
                {
                    try
                    {
                        logger.WriteToFile(" trade date " + trade.Date + " " + "period " + trade.Periods[i].Period + " volume:" + trade.Periods[i].Volume);
                        outputArray[i] = outputArray[i] + trade.Periods[i].Volume;
                    }
                    catch (Exception ex)
                    {
                        logger.WriteToFile(" error while processing trade data " + ex.Message + "  " + DateTime.Now);
                        exceptions.Add(ex);
                    }
                }
            }

            tradeData = outputArray;

            for (int i = 0; i < 24; i++)
            {
                logger.WriteToFile(" final output " + i + " " + outputArray[i]);
            }

            if (exceptions.Count > 0)
                throw new AggregateException(exceptions);

            
        }

        private void PowerServiceCall()
        {
            Services.IPowerService powerService = new Services.PowerService();
            trades = powerService.GetTrades(DateTime.UtcNow);
        }

        public void TryPowerServiceCall(Action action)
        {
            var tries = 3;
            while (true)
            {
                try
                {
                    action();
                    break; // success!
                }
                catch(Exception ex)
                {
                    if (--tries == 0)
                    {
                        logger.WriteToFile(" error calling power service three attempts failed " + ex.Message  + "  " + DateTime.Now);
                        throw new ApplicationException(ex.Message);
                    }
                        
                    Thread.Sleep(5000);
                }
            }
        }

        public double[] GetTradeData()
        {
            GetData();
            return tradeData;
        }
        
    }
}
