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

            return bank.mergeBank(createBank(parseData(data)));
        }

        public String[] parseData(String data)
        {
            return data.Split("\n").Skip(1).SkipLast(1).ToArray();
        }

        public Bank createBank(String[] data)
        {
            Bank bank = new Bank();

            foreach (String line in data)
            {
                String[] lineData = line.Split(",");

                //check the first account
                if (!bank.accountList.ContainsKey(lineData[1]))
                {
                    bank.setAccount(lineData[1], new Account(lineData[1]));
                }
                //check the second account
                if (!bank.accountList.ContainsKey(lineData[2]))
                {
                    bank.setAccount(lineData[2], new Account(lineData[1]));
                }

                try
                {
                    bank.getAccount(lineData[1]).TotalAmount -= Double.Parse(lineData[4]);
                    bank.getAccount(lineData[2]).TotalAmount = Double.Parse(lineData[4]);
                    Double parsedAmount = Double.Parse(lineData[4]);
                    bank.addTransaction(lineData[1], new Transaction(DateTime.Parse(lineData[0]), lineData[1], lineData[2], lineData[3], parsedAmount));
                    bank.addTransaction(lineData[2], new Transaction(DateTime.Parse(lineData[0]), lineData[1], lineData[2], lineData[3], parsedAmount));
                }
                catch (Exception e)
                {
                    logger.Debug("Uh oh, something happened: the program tried to add a transaction and it failed.");
                    logger.Error("Error:\n" + e + "\n\n" + line + "\n\n");
                }
            }

            return bank;
        }
    }
}
