using System;
using System.Collections.Generic;
using System.Text;

namespace supportbank
{
    class Bank
    {
        public Dictionary<String, Account> accountList { get; set; }

        public Bank()
        {
            accountList = new Dictionary<String, Account>();
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
