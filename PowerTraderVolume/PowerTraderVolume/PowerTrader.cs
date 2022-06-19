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
using System.Globalization;

namespace PowerTraderVolume
{
    public partial class PowerTrader : ServiceBase
    {
        Timer timer = new Timer();
        int timeIntervalInMinites;
        string filePath;
        Logger logger;
        PowerService powerService;
        CSVWritter csvWritter;
        public PowerTrader()
        {
            InitializeComponent();
            logger = new Logger();
            powerService = new PowerService();
            csvWritter = new CSVWritter();
        }

        private void LoadConfigValues()
        {
            timeIntervalInMinites = Int32.Parse(ConfigurationManager.AppSettings["TimeIntervalInMinites"].ToString());
            filePath = ConfigurationManager.AppSettings["FilePath"].ToString();
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = timeIntervalInMinites * 60000; //number in milisecinds  
            timer.Enabled = true;
        }

        protected override void OnStart(string[] args)
        {
            logger.WriteToFile("Service is started at " + DateTime.Now);
            LoadConfigValues();
            PopulateTradeData();
        }


        protected override void OnStop() {
            logger.WriteToFile("Service is stopped at " + DateTime.Now);  
        }  
        private void OnElapsedTime(object source, ElapsedEventArgs e) {
            PopulateTradeData();
        }
        
        private void PopulateTradeData()
        {
            logger.WriteToFile("Service is recall at " + DateTime.Now);
            try
            {
                var tradeData = powerService.GetTradeData();
                DateTime now = DateTime.UtcNow;
                string fileName = "PowerPosition_" + now.ToString("yyyyMMdd") + "_" + now.ToString("HHmm") + ".csv";
                csvWritter.WriteData(filePath, fileName, tradeData);
            }
            catch (ApplicationException appEx)
            {
                logger.WriteToFile("Power Service Call failed three times, alert production support  " + appEx.Message + " " + DateTime.Now);
            }
            catch (AggregateException aggEx)
            {
                logger.WriteToFile("Trade Data Error, alert production support and refer logs for detail " + DateTime.Now);
            }
            catch (Exception ex)
            {
                logger.WriteToFile("Unexpected Error " + ex.Message + " " + DateTime.Now);
            }

        }
    }
}
