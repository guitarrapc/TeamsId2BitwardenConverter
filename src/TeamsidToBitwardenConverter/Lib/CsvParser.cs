using CsvHelper;
using System;
using System.IO;
using System.Linq;

namespace TeamsidToBitwardenConverter.Lib
{
    public class CsvParser
    {
        private readonly string path;

        public CsvParser(string path)
        {
            this.path = path;
        }

        public T[] Parse<T>() where T : class
        {
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.MissingFieldFound = null;
                var records = csv.GetRecords<T>();
                return records.ToArray();
            }
        }
    }
}
