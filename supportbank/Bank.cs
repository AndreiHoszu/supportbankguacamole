using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace supportbank
{
    class Bank
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public Dictionary<String, Account> accountList { get; set; }

        public Bank()
        {
            accountList = new Dictionary<String, Account>();
        }

        public Bank(String[] data)
        {
            accountList = new Dictionary<String, Account>();

            foreach (String line in data)
            {
                String[] lineData = line.Split(",");

                //check the first account
                if(!accountList.ContainsKey(lineData[1]))
                {
                    setAccount(lineData[1], new Account(lineData[1]));
                }
                //check the second account
                if (!accountList.ContainsKey(lineData[2]))
                {
                    setAccount(lineData[2], new Account(lineData[1]));
                }

                try
                {
                    getAccount(lineData[1]).TotalAmount -= Double.Parse(lineData[4]);
                    getAccount(lineData[2]).TotalAmount = Double.Parse(lineData[4]);
                    Double parsedAmount = Double.Parse(lineData[4]);
                    addTransaction(lineData[1], new Transaction(DateTime.Parse(lineData[0]), lineData[1], lineData[2], lineData[3], parsedAmount));
                    addTransaction(lineData[2], new Transaction(DateTime.Parse(lineData[0]), lineData[1], lineData[2], lineData[3], parsedAmount));
                }
                catch (Exception e)
                {
                    logger.Debug("Uh oh, something happened: the program tried to add a transaction and it failed.");
                    logger.Error("Error:\n" + e + "\n\n" + line + "\n\n");
                }
            }
        }

        public Account getAccount(String accountName)
        {
            if(accountList.ContainsKey(accountName))
            {
                return accountList[accountName];
            }

            return null;
        }

        public void setAccount(String accountName, Account account)
        {
            accountList[accountName] = account;
        }

        public void addTransaction(String accountName, Transaction transaction)
        {
            if(accountList.ContainsKey(accountName))
            {
                accountList[accountName].addTransaction(transaction);
            }
        }

        public List<String> getKeys()
        {
            return new List<String>(accountList.Keys);
        }

        public Bank mergeBank(Bank bank2)
        {
            foreach (String accountName in bank2.getKeys())
            {
                if(accountList.ContainsKey(accountName))
                {
                    //this happens if both banks contain an account with the same name, so we have to merge them togheter

                    Account account1 = this.getAccount(accountName);
                    Account account2 = bank2.getAccount(accountName);

                    foreach (Transaction transaction in account2.transactions)
                    {
                        //so we add each transaction from the second bank's account to the first bank's account
                        account1.addTransaction(transaction);
                    }

                    this.setAccount(accountName, account1);
                }
                else
                {
                    this.setAccount(accountName, bank2.getAccount(accountName));
                }
            }

            return this;
        }
    }
}
