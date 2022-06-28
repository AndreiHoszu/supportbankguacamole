using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace supportbank
{
    class Program
    {
        static String readDataFromCVS()
        {
            String data;

            try
            {
                data = File.ReadAllText(@"C:\Work\Training\supportbank\supportbank\Transactions2014.csv");
            }
            catch(Exception e)
            {
                return "";
            }

            return data;
        }

        static Dictionary<String, double> calculateTransactions(String[] dataLines)
        {
            Dictionary<String, double> accountList = new Dictionary<string, double>();

            foreach (String line in dataLines)
            {
                String[] lineData = line.Split(",");

                if (accountList.ContainsKey(lineData[1]))
                {
                    accountList[lineData[1]] -= Double.Parse(lineData[4]);
                }
                else
                {
                    accountList.Add(lineData[1], -Double.Parse(lineData[4]));
                }
                if (accountList.ContainsKey(lineData[2]))
                {
                    accountList[lineData[2]] += Double.Parse(lineData[4]);
                }
                else
                {
                    accountList.Add(lineData[2], +Double.Parse(lineData[4]));
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

        static void Main(string[] args)
        {
            Dictionary<String, double> accountList = new Dictionary<string, double>();

            String dataText = readDataFromCVS();
            String[] dataLines = dataText.Split("\n").Skip(1).SkipLast(1).ToArray();

            accountList = calculateTransactions(dataLines);

            Console.WriteLine("List All or List [Account]?");

            String userInput = Console.ReadLine();
            List<String> keyList = new List<String>(accountList.Keys);

            if(userInput == "List All")
            {
                foreach(String key in keyList)
                {
                    displayStatus(accountList, key);
                }
            }

            String accountName = String.Join(" ", userInput.Split(" ").Skip(1).ToArray());

            if(accountList.ContainsKey(accountName))
            {
                Console.WriteLine("Account found.");

                displayStatus(accountList, accountName);

                Console.WriteLine("List of transactions:");

                foreach (String line in dataLines)
                {
                    String[] lineData = line.Split(",");

                    if(lineData[1] == accountName)
                    {
                        Console.WriteLine(accountName + " sent " + lineData[4] + " to " + lineData[2] + " on " + lineData[0] + "; Narrative: " + lineData[3]);
                    }
                    if(lineData[2] == accountName)
                    {
                        Console.WriteLine(accountName + " received " + lineData[4] + " from " + lineData[1] + " on " + lineData[0] + "; Narrative: " + lineData[3]);
                    }
                }
            }
            else
            {
                Console.WriteLine("Account was not found.");
            }
        }
    }
}
