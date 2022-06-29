using System;
using System.Collections.Generic;
using System.Text;

namespace supportbank
{
    class Account
    {
        public Double TotalAmount { get; set; }
        public String AccountName { get; }
        public List<Transaction> transactions { get; }

        public Account(String accountName)
        {
            AccountName = accountName;
            transactions = new List<Transaction>();
            TotalAmount = 0;
        }

        public void addTransaction(Transaction transaction)
        {
            transactions.Add(transaction);

            if (transaction.From == AccountName)
            {
                TotalAmount -= transaction.Amount;
            }
            else if (transaction.To == AccountName)
            {
                TotalAmount += transaction.Amount;
            }
        }

        public void calculateAmount()
        {
            double owes = 0;
            double owed = 0;

            foreach(Transaction transaction in transactions)
            {
                if(transaction.From == AccountName)
                {
                    owes -= transaction.Amount;
                }
                if(transaction.To == AccountName)
                {
                    owed += transaction.Amount;
                }
            }

            TotalAmount = owes + owed;
        }
    }
}
