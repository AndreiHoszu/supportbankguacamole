using System;
using System.Collections.Generic;
using System.Text;

namespace supportbank
{
    class Transaction
    {
        public DateTime Date { get; set; }

        public String From { get; set; }

        public String To { get; set; }

        public String Narrative { get; set; }

        public Double Amount { get; set; }

        public Transaction(DateTime date, String from, String to, String narrative, Double amount)
        {
            this.Date = date;
            this.From = from;
            this.To = to;
            this.Narrative = narrative;
            this.Amount = amount;
        }
    }
}
