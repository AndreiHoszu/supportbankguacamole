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

        static Bank parseDataFromFiles(String[] paths)
        {
            Bank bank = new Bank();

            CSVParser csvParser = new CSVParser();
            JSONParser jsonParser = new JSONParser();
            XMLParser xmlParser = new XMLParser();

            foreach (String path in paths)
            {
                String format = path.Substring(path.LastIndexOf("."));

                switch(format)
                {
                    case ".csv":
                        bank = csvParser.parseFile(bank, path);
                        break;
                    case ".json":
                        //dates will be converted to CVS format: day/month/year
                        bank = jsonParser.parseFile(bank, path);
                        break;
                    case ".xml":
                        //dates will be converted to CVS format: day/month/year
                        bank = xmlParser.parseFile(bank, path);
                        break;
                    default:
                        Console.WriteLine("Unsuported file type " + format);
                        logger.Info("Program tried to read a file that is not supported.");
                        break;
                }
            }

            return bank;
        }

        static void displayStatus(Bank bank, String key)
        {
            bank.getAccount(key).calculateAmount();

            if (bank.getAccount(key).TotalAmount > 0)
            {
                Console.WriteLine(key + " is owed " + Math.Round(bank.getAccount(key).TotalAmount, 2));
            }
            else if(bank.getAccount(key).TotalAmount < 0)
            {
                Console.WriteLine(key + " owes " + Math.Round(bank.getAccount(key).TotalAmount, 2));
            }
            else
            {
                Console.WriteLine(key + " does not owe anything.");
            }
        }

        static void displayTransactions(Bank bank, String accountName)
        {
            if(bank.accountList.ContainsKey(accountName))
            {
                Console.WriteLine("Account found.");

                displayStatus(bank, accountName);

                Console.WriteLine("List of transactions:");

                foreach(Transaction transaction in bank.getAccount(accountName).transactions)
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

                Console.WriteLine("Testing\n");
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

            Bank bank = new Bank();
            bank = parseDataFromFiles(paths);

            logger.Info("Program successfully read the data from the specified files.");

            logger.Info("Program successfully calculated the transactions.");

            Console.WriteLine("List All or List [Account]?");
            String userInput = Console.ReadLine();
            List<String> keyList = new List<String>(bank.getKeys());

            if(userInput == "List All")
            {
                foreach(String key in keyList)
                {
                    displayStatus(bank, key);
                    logger.Info("Successfully displayed each account's status.");
                }
            }

            String accountName = String.Join(" ", userInput.Split(" ").Skip(1).ToArray());
            if(accountName != "All")
            {
                displayTransactions(bank, accountName);
                logger.Info("Successfully displayed account's status.");
            }

            logger.Info("Program ended successfully.");
        }
    }
}
