using System;
using System.Collections.Generic;
using System.Text;

namespace supportbank
{
    class Account
    {
        public Account(String accountName)
        {
            AccountName = accountName;
            transactions = new List<Transaction>();
            TotalAmount = 0;
        }
        public Double TotalAmount { get; set; }

        public String AccountName { get; }

        public List<Transaction> transactions { get; }

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
    }
}
