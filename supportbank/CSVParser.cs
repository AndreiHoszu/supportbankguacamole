using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace supportbank
{
    class CSVParser : IParser
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

            return mergeBanks(bank, createBank(parseData(data)));
        }

        public String[] parseData(String data)
        {
            return data.Split("\n").Skip(1).SkipLast(1).ToArray();
        }

        public Bank createBank(String[] data)
        {
            Bank temp = new Bank();

            foreach (String line in data)
            {
                String[] lineData = line.Split(",");

                //check the first account
                if (!temp.accountList.ContainsKey(lineData[1]))
                {
                    temp.setAccount(lineData[1], new Account(lineData[1]));
                }
                //check the second account
                if (!temp.accountList.ContainsKey(lineData[2]))
                {
                    temp.setAccount(lineData[2], new Account(lineData[1]));
                }

                try
                {
                    temp.getAccount(lineData[1]).TotalAmount -= Double.Parse(lineData[4]);
                    temp.getAccount(lineData[2]).TotalAmount = Double.Parse(lineData[4]);
                    Double parsedAmount = Double.Parse(lineData[4]);
                    temp.addTransaction(lineData[1], new Transaction(DateTime.Parse(lineData[0]), lineData[1], lineData[2], lineData[3], parsedAmount));
                    temp.addTransaction(lineData[2], new Transaction(DateTime.Parse(lineData[0]), lineData[1], lineData[2], lineData[3], parsedAmount));
                }
                catch (Exception e)
                {
                    logger.Debug("Uh oh, something happened: the program tried to add a transaction and it failed.");
                    logger.Error("Error:\n" + e + "\n\n" + line + "\n\n");
                }
            }

            return temp;
        }

        public Bank mergeBanks(Bank bank1, Bank bank2)
        {
            foreach(String accountName in bank2.getKeys())
            {
                if(bank1.accountList.ContainsKey(accountName))
                {
                    //this happens if both banks contain an account with the same name, so we have to merge them togheter

                    Account temp1 = bank1.getAccount(accountName);
                    Account temp2 = bank2.getAccount(accountName);

                    foreach(Transaction transaction in temp2.transactions)
                    {
                        //so we add each transaction from the second bank's account to the first bank's account
                        temp1.addTransaction(transaction);
                    }

                    bank1.setAccount(accountName, temp1);
                }
                else
                {
                    bank1.setAccount(accountName, bank2.getAccount(accountName));
                }
            }

            return bank1;
        }
    }
}
