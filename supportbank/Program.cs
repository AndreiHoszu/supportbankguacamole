using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace supportbank
{
    class Program
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        static Dictionary<String, Account> parseDataFromFiles(String[] paths)
        {
            String[] data = { };

            foreach(String path in paths)
            {
                String format = path.Substring(path.LastIndexOf("."));

                switch(format)
                {
                    case ".csv":
                        data = data.Concat(readDataFromCVS(path)).ToArray();
                        break;
                    case ".json":
                        //dates will be converted to CVS format: day/month/year
                        data = data.Concat(readDataFromJSON(path)).ToArray();
                        break;
                    case ".xml":
                        //dates will be converted to CVS format: day/month/year
                        data = data.Concat(readDataFromXML(path)).ToArray();
                        break;
                    default:
                        Console.WriteLine("Unsuported file type " + format);
                        logger.Info("Program tried to read a file that is not supported.");
                        break;
                }
            }

            Dictionary<String, Account> accounts = new Dictionary<String, Account>();

            foreach (String line in data)
            {
                String[] lineData = line.Split(",");

                //checks the first account
                if(!accounts.ContainsKey(lineData[1]))
                {
                    accounts.Add(lineData[1], new Account(lineData[1]));
                }

                try
                {
                    accounts[lineData[1]].TotalAmount -= Double.Parse(lineData[4]);
                    int day = Int32.Parse(lineData[0].Substring(0, 2));
                    int month = Int32.Parse(lineData[0].Substring(3, 2));
                    int year = Int32.Parse(lineData[0].Substring(6, 4));
                    DateTime parsedDate = new DateTime(year, month, day);
                    Double parsedAmount = Double.Parse(lineData[4]);
                    accounts[lineData[1]].addTransaction(new Transaction(parsedDate, lineData[1], lineData[2], lineData[3], parsedAmount));
                }
                catch (Exception e)
                {
                    logger.Debug("Uh oh, something happened: the program tried to add a transaction and it failed.");
                    logger.Error("Error:\n" + e + "\n\n"+line+"\n\n");
                }

                //checks the second account
                if(!accounts.ContainsKey(lineData[2]))
                {
                    accounts.Add(lineData[2], new Account(lineData[1]));
                }

                try
                {
                    accounts[lineData[2]].TotalAmount = Double.Parse(lineData[4]);
                    int day = Int32.Parse(lineData[0].Substring(0, 2));
                    int month = Int32.Parse(lineData[0].Substring(3, 2));
                    int year = Int32.Parse(lineData[0].Substring(6, 4));
                    DateTime parsedDate = new DateTime(year, month, day);
                    Double parsedAmount = Double.Parse(lineData[4]);
                    accounts[lineData[2]].addTransaction(new Transaction(parsedDate, lineData[1], lineData[2], lineData[3], parsedAmount));
                }
                catch (Exception e)
                {
                    logger.Debug("Uh oh, something happened: the program tried to add a transaction and it failed.");
                    logger.Error("Error:\n" + e + "\n\n" + line + "\n\n");
                }
            }

            return accounts;
        }

        static String[] readDataFromCVS(String path)
        {
            String data;

            try
            {
                data = File.ReadAllText(path);
            }
            catch(Exception e)
            {
                logger.Debug("Uh oh, something happened: the program tried to read data from a CVS but it failed(maybe the file is already opened?).");
                logger.Error("Error:\n" + e);
                return null;
            }

            return data.Split("\n").Skip(1).SkipLast(1).ToArray();
        }

        static String[] readDataFromJSON(String path)
        {
            String data = null;
            try
            {
                data = File.ReadAllText(path);
            }
            catch (Exception e)
            {
                logger.Debug("Uh oh, something happened: the program tried to read data from a JSON but it failed(maybe the file is already opened?).");
                logger.Error("Error:\n" + e);
                return null;
            }
            List<JToken> transactions = JsonConvert.DeserializeObject<List<JToken>>(data);

            String[] returnData = new String[transactions.Count];
            int counter = 0;

            foreach(JToken transaction in transactions)
            {
                //we need to convert from ISO-8601 date(YYYY-MM-DDTHH-MM-SS.MMMZ) format to DD/MM/YYYY
                //by doing this we will lose the timestamps
                //but this is not a big deal since the other date formats only count up to days and not miliseconds
                //or we could do the other way around and add midnight timestamps to all other datetimes
                String convertedDate = (transaction["Date"] + "").Substring(0, 10); ;//it is really strange really

                //                     days                   /             months                  /               years
                //convertedDate = convertedDate.Substring(8) + "/" + convertedDate.Substring(4, 2) + "/" + convertedDate.Substring(0, 4);

                returnData[counter++] = convertedDate + "," + transaction["FromAccount"] + "," + transaction["ToAccount"] + "," + transaction["Narrative"] + "," + transaction["Amount"];
            }

            return returnData;
        }

        static String[] readDataFromXML(String path)
        {
            String data = null;
            try
            {
                data = File.ReadAllText(path);
            }
            catch (Exception e)
            {
                logger.Debug("Uh oh, something happened: the program tried to read data from an XML but it failed(maybe the file is already opened?).");
                logger.Error("Error:\n" + e);
                return null;
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(data);

            String[] returnData = new String[doc.LastChild.ChildNodes.Count];
            int counter = 0;

            foreach (XmlNode node in doc.LastChild.ChildNodes)
            {
                String convertedDate = node.Attributes["Date"]?.InnerText;
                DateTime temp = DateTime.FromOADate(Double.Parse(convertedDate));
                String day = temp.Day + "";
                if(day.Length == 1)
                {
                    day = "0" + day;
                }
                String month = temp.Month + "";
                if(month.Length == 1)
                {
                    month = "0" + month;
                }

                convertedDate = day + "/" + month + "/" + temp.Year;

                returnData[counter++] = convertedDate + "," + node.ChildNodes[2].ChildNodes[0].InnerText + "," + node.ChildNodes[2].ChildNodes[1].InnerText + "," + node.ChildNodes[0].InnerText + "," + node.ChildNodes[1].InnerText;
            }

            return returnData;
        }

        static void displayStatus(Dictionary<String, Account> accountList, String key)
        {
            if(accountList[key].TotalAmount > 0)
            {
                Console.WriteLine(key + " is owed " + accountList[key].TotalAmount);
            }
            else if(accountList[key].TotalAmount < 0)
            {
                Console.WriteLine(key + " owes " + accountList[key].TotalAmount);
            }
            else
            {
                Console.WriteLine(key + " does not owe anything.");
            }
        }

        static void displayTransactions(Dictionary<String, Account> accountList, String accountName)
        {
            if(accountList.ContainsKey(accountName))
            {
                Console.WriteLine("Account found.");

                displayStatus(accountList, accountName);

                Console.WriteLine("List of transactions:");

                foreach(Transaction transaction in accountList[accountName].transactions)
                {
                    if(transaction.From == accountName)
                    {
                        Console.WriteLine(accountName + " sent " + transaction.Amount + " to " + transaction.To + " on " + transaction.Date + "; Narrative: " + transaction.Narrative);
                    }
                    else if(transaction.To == accountName)
                    {
                        Console.WriteLine(accountName + " received " + transaction.Amount + " from " + transaction.From + " on " + transaction.Date + "; Narrative: " + transaction.Narrative);
                    }
                }
            }
            else
            {
                Console.WriteLine("Account was not found.");
                logger.Info("No known account matched the input.");
            }
        }

        static void Main(string[] args)
        {
            var config = new LoggingConfiguration();
            var target = new FileTarget { FileName = @"C:\Work\Logs\SupportBank.log", Layout = @"${longdate} ${level} - ${logger}: ${message}" };
            config.AddTarget("File Logger", target);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target));
            LogManager.Configuration = config;
            logger.Info("Program started. Hooray!");

            String[] paths = { @"C:\Work\Training\supportbank\supportbank\Transactions2014.csv", @"C:\Work\Training\supportbank\supportbank\DodgyTransactions2015.csv" ,
            @"C:\Work\Training\supportbank\supportbank\Transactions2013.json", @"C:\Work\Training\supportbank\supportbank\Transactions2012.xml"};
            Dictionary<String, Account> accountList = parseDataFromFiles(paths);
            logger.Info("Program successfully read the data from the specified files.");

            logger.Info("Program successfully calculated the transactions.");

            Console.WriteLine("List All or List [Account]?");
            String userInput = Console.ReadLine();
            List<String> keyList = new List<String>(accountList.Keys);

            if(userInput == "List All")
            {
                foreach(String key in keyList)
                {
                    displayStatus(accountList, key);
                    logger.Info("Successfully displayed each account's status.");
                }
            }

            String accountName = String.Join(" ", userInput.Split(" ").Skip(1).ToArray());
            if(accountName != "All")
            {
                displayTransactions(accountList, accountName);
                logger.Info("Successfully displayed account's status.");
            }

            logger.Info("Program ended successfully.");
        }
    }
}
