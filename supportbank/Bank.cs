using System;
using System.Collections.Generic;
using System.Text;

namespace supportbank
{
    class Bank
    {
        public Bank()
        {
            accountList = new Dictionary<String, Account>();
        }

        public Dictionary<String, Account> accountList { get; set; }

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
    }
}
