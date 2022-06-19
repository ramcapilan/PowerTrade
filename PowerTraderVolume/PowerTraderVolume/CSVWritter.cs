using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerTraderVolume
{
    public class CSVWritter
    {
        private Logger logger;
        public CSVWritter()
        {
            logger = new Logger();
        }
        public void WriteData(string path, string fileName, double[] tradesVolume)
        {
            try
            {
                var dataClasslist = GetData(tradesVolume);
                DirectoryInfo di = Directory.CreateDirectory(path);

                using (StreamWriter sw = new StreamWriter(Path.Combine(path, fileName), true))
                {
                    using (var csv = new CsvWriter(sw, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(dataClasslist);
                        sw.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.WriteToFile(" error while writting trade data " + ex.Message + "  " + DateTime.Now);
            }

        }

        private List<TradeVolume> GetData(double[] outputArray)
        {
            var dataClasslist = new List<TradeVolume>();

            string[] localTime = new string[24] { "23.00", "00.00", "01.00", "02.00", "03.00", "04.00", "05.00", "06.00", "07.00", "08.00", "09.00", "10.00", "11.00", "12.00", "13.00", "14.00", "15.00", "16.00", "17.00", "18.00", "19.00", "20.00", "21.00","22.00" };


            for (int i = 0; i < 24; i++)
            {
                dataClasslist.Add(new TradeVolume { LocalTime = localTime[i], Volume = (int)outputArray[i] });
            }
            return dataClasslist;
        }

    }
}
