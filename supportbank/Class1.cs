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

        public void addTransaction(Transaction transaction)
        {
            transactions.Add(transaction);

            if(transaction.From == AccountName)
            {
                TotalAmount -= transaction.Amount;
            }
            else if(transaction.To == AccountName)
            {
                TotalAmount += transaction.Amount;
            }
        }

        public Double TotalAmount { get; set; }

        public String AccountName { get; }

        public List<Transaction> transactions { get; }
    }

    class Transaction
    {
        public Transaction(DateTime date, String from, String to, String narrative, Double amount)
        {
            this.Date = date;
            this.From = from;
            this.To = to;
            this.Narrative = narrative;
            this.Amount = amount;
        }

        public DateTime Date { get; set; }

        public String From { get; set; }

        public String To { get; set; }

        public String Narrative { get; set; }

        public Double Amount { get; set; }
    }
}
