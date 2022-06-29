using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace supportbank
{
    class XMLParser: IParser
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
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(data);

            String[] returnData = new String[doc.LastChild.ChildNodes.Count];
            int counter = 0;

            foreach (XmlNode node in doc.LastChild.ChildNodes)
            {
                String convertedDate = node.Attributes["Date"]?.InnerText;
                DateTime temp = DateTime.FromOADate(Double.Parse(convertedDate));
                String day = temp.Day + "";
                if (day.Length == 1)
                {
                    day = "0" + day;
                }
                String month = temp.Month + "";
                if (month.Length == 1)
                {
                    month = "0" + month;
                }

                convertedDate = day + "/" + month + "/" + temp.Year;

                returnData[counter++] = convertedDate + "," + node.ChildNodes[2].ChildNodes[0].InnerText + "," + node.ChildNodes[2].ChildNodes[1].InnerText + "," + node.ChildNodes[0].InnerText + "," + node.ChildNodes[1].InnerText;
            }

            return returnData;
        }
    }
}
