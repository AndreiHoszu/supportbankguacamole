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

        static String[] readDataFromFiles(String[] paths)
        {
            //TODO
            String[] data = { };

            foreach(String path in paths)
            {
                Console.WriteLine("file number 1?");
                String format = path.Substring(path.LastIndexOf("."));

                switch(format)
                {
                    case ".csv":
                        data = data.Concat(readDataFromCVS(path)).ToArray();
                        break;
                    case ".json":
                        data = data.Concat(readDataFromJSON(path)).ToArray();
                        break;
                    case ".xml":
                        data = data.Concat(readDataFromXML(path)).ToArray();
                        break;
                    default:
                        Console.WriteLine("Unsuported file type " + format);
                        logger.Info("Program tried to read a file that it does not support.");
                        break;
                }
            }

            return data;
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

            return data.Split("\n").Skip(1).SkipLast(1).ToArray(); ;
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
                returnData[counter++] = transaction["Date"] + "," + transaction["FromAccount"] + "," + transaction["ToAccount"] + "," + transaction["Narrative"] + "," + transaction["Amount"];
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

            //XmlSerializer

            String[] returnData = new String[doc.LastChild.ChildNodes.Count];
            int counter = 0;

            foreach (XmlNode node in doc.LastChild.ChildNodes)
            {
                returnData[counter++] = node.Attributes["Date"]?.InnerText + "," + node.ChildNodes[2].ChildNodes[0].InnerText + "," + node.ChildNodes[2].ChildNodes[1].InnerText + "," + node.ChildNodes[0].InnerText + "," + node.ChildNodes[1].InnerText;
            }

            return returnData;
        }

        static Dictionary<String, double> calculateTransactions(Dictionary<String, double> accountList, String[] dataLines)
        {
            foreach (String line in dataLines)
            {
                String[] lineData = line.Split(",");

                if (accountList.ContainsKey(lineData[1]))
                {
                    try
                    {
                        accountList[lineData[1]] -= Double.Parse(lineData[4]);
                    }
                    catch (Exception e)
                    {
                        logger.Debug("Uh oh, something happened: the program tried to convert something that wasn't a number into a variable of type Double.");
                        logger.Error("Error:\n" + e);
                    }
                }
                else
                {
                    try
                    {
                        accountList.Add(lineData[1], -Double.Parse(lineData[4]));
                    }
                    catch (Exception e)
                    {
                        logger.Debug("Uh oh, something happened: the program tried to convert something that wasn't a number into a variable of type Double.");
                        logger.Error("Error:\n" + e);
                    }
                }
                if (accountList.ContainsKey(lineData[2]))
                {
                    try
                    {
                        accountList[lineData[2]] += Double.Parse(lineData[4]);
                    }
                    catch (Exception e)
                    {
                        logger.Debug("Uh oh, something happened: the program tried to convert something that wasn't a number into a variable of type Double.");
                        logger.Error("Error:\n" + e);
                    }
            }
                else
                {
                    try
                    {
                        accountList.Add(lineData[2], +Double.Parse(lineData[4]));
                    }
                    catch (Exception e)
                    {
                        logger.Debug("Uh oh, something happened: the program tried to convert something that wasn't a number into a variable of type Double.");
                        logger.Error("Error:\n" + e);
                    }
                }
            }

            return accountList;
        }

        static void displayStatus(Dictionary<String, double> accountList, String key)
        {
            if (accountList[key] > 0)
            {
                Console.WriteLine(key + " is owed " + accountList[key]);
            }
            else if (accountList[key] < 0)
            {
                Console.WriteLine(key + " owes " + accountList[key]);
            }
            else
            {
                Console.WriteLine(key + " does not owe anything.");
            }
        }

        static void displayTransactions(Dictionary<String, double> accountList, String[] dataLines, String accountName)
        {
            if (accountList.ContainsKey(accountName))
            {
                Console.WriteLine("Account found.");

                displayStatus(accountList, accountName);

                Console.WriteLine("List of transactions:");

                foreach (String line in dataLines)
                {
                    String[] lineData = line.Split(",");

                    if (lineData[1] == accountName)
                    {
                        Console.WriteLine(accountName + " sent " + lineData[4] + " to " + lineData[2] + " on " + lineData[0] + "; Narrative: " + lineData[3]);
                    }
                    else if (lineData[2] == accountName)
                    {
                        Console.WriteLine(accountName + " received " + lineData[4] + " from " + lineData[1] + " on " + lineData[0] + "; Narrative: " + lineData[3]);
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
            Dictionary<String, double> accountList = new Dictionary<string, double>();

            String[] paths = { @"C:\Work\Training\supportbank\supportbank\Transactions2014.csv", @"C:\Work\Training\supportbank\supportbank\DodgyTransactions2015.csv" ,
            @"C:\Work\Training\supportbank\supportbank\Transactions2013.json", @"C:\Work\Training\supportbank\supportbank\Transactions2012.xml"};

            String[] dataLines = readDataFromFiles(paths);

            logger.Info("Program successfully read the data from the specified files.");

            accountList = calculateTransactions(accountList, dataLines);

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
                displayTransactions(accountList, dataLines, accountName);
                logger.Info("Successfully displayed account's status.");
            }

            logger.Info("Program ended successfully.");
        }
    }
}
