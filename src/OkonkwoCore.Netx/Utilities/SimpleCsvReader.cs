using NotVisualBasic.FileIO;
using System.Collections.Generic;
using System.Text;

namespace OkonkwoCore.Netx.Utilities
{
    /// <summary>
    /// This simple csv reader wraps the CsvTextFieldParser.
    /// The reader parses csv files and returns the rows as a collection delimited strings.
    /// For additional information, see the GitHub: https://github.com/22222/CsvTextFieldParser
    /// </summary>
    public static class SimpleCsvReader
    {
        public static IEnumerable<string[]> GetRows(string fileUri, char delimiter,
            string firstHeader = null, bool returnHeaders = true, bool fieldsEnclosedInQuotes = true, bool trimWhiteSpace = true)
        {
            var parser = new CsvTextFieldParser(fileUri, Encoding.UTF8)
            {
                HasFieldsEnclosedInQuotes = fieldsEnclosedInQuotes,
                CompatibilityMode = trimWhiteSpace
            };

            parser.SetDelimiter(delimiter);

            var rowsList = new List<string[]>();

            while (!parser.EndOfData)
            {
                string[] row = parser.ReadFields();

                if (firstHeader != null)
                {
                    bool isHeader = row[0].ToLower() == firstHeader.ToLower();

                    if (isHeader && !returnHeaders)
                        continue;
                }

                rowsList.Add(row);
            }

            return rowsList;
        }
    }
}
