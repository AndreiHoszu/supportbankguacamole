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

            return bank.mergeBank(new Bank(parseData(data)));
        }

        public String[] parseData(String data)
        {
            return data.Split("\n").Skip(1).SkipLast(1).ToArray();
        }
    }
}
