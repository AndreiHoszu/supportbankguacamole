using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace supportbank
{
    class JSONParser: IParser
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public Bank parseFile(Bank bank, String path)
        {
            String data = "";

            try
            {
                data = File.ReadAllText(path);
            }
            catch (Exception e)
            {
                logger.Debug("Uh oh, something happened: the program tried to read data from a CVS but it failed(maybe the file is already opened?).");
                logger.Error("Error:\n" + e);
                return null;
            }

            return bank.mergeBank(new Bank(parseData(data)));
        }

        public String[] parseData(String data)
        {
            List<JToken> transactions = JsonConvert.DeserializeObject<List<JToken>>(data);

            String[] returnData = new String[transactions.Count];
            int counter = 0;

            foreach (JToken transaction in transactions)
            {
                //we need to convert from ISO-8601 date(YYYY-MM-DDTHH-MM-SS.MMMZ) format to DD/MM/YYYY
                //by doing this we will lose the timestamps
                //but this is not a big deal since the other date formats only count up to days and not miliseconds
                //or we could do the other way around and add midnight timestamps to all other datetimes
                String convertedDate = (transaction["Date"] + "").Substring(0, 10);

                returnData[counter++] = convertedDate + "," + transaction["FromAccount"] + "," + transaction["ToAccount"] + "," + transaction["Narrative"] + "," + transaction["Amount"];
            }

            return returnData;
        }
    }
}
