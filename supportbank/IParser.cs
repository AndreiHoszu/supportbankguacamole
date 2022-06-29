using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace supportbank
{
    interface IParser
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public Bank parseFile(Bank bank, String path);

        public String[] parseData(String data);

        public Bank createBank(String[] data);

        public Bank mergeBanks(Bank bank1, Bank bank2);
    }
}
