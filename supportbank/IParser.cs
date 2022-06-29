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
    }
}
