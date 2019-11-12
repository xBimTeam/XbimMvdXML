using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Xbim.MvdXml.DataManagement
{
    public class MvdPropertyRuleValue
    { 
        public DataIndicator DataIndicator;
        public string DataValue;
        public string DataComparison;

        public MvdPropertyRuleValue(string varDataIndicator, string varDataValue, string varDataComparison)
        {
            DataIndicator = new DataIndicator(varDataIndicator);
            DataComparison = varDataComparison;
            DataValue = varDataValue;
        }

        public override string ToString()
        {
            return $"{DataIndicator}{DataComparison}{DataValue}";
        }

        private static readonly Regex Re = new Regex(@" *(?<varDI>.+?) *(?<cmpRule>[!=\<\>]+) *(?<varVal>.*) *");
        private static readonly Regex ReOper = new Regex(@"( OR | AND | XOR | NOR | NAND | NXOR )", RegexOptions.IgnoreCase);

        internal static IEnumerable<MvdPropertyRuleValue> GetValues(string storageString)
        {
            var vals = new List<MvdPropertyRuleValue>();
            var parts = ReOper.Split(storageString)
                .Where(p => !ReOper.IsMatch(p))
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToArray();
           
            foreach (var part in parts)
            {
                var v = Re.Match(part.Trim());
                if (v.Success)
                {
                    var rv = new MvdPropertyRuleValue(
                        v.Groups["varDI"].Value,
                        v.Groups["varVal"].Value,
                        v.Groups["cmpRule"].Value
                        );
                    vals.Add(rv);
                }
            }
            return vals;
        }

        // todo: will probably have to implement the full grammar to parse and convert the string.
        // todo: numeric values will have to be implemeted in the datatable extraction, if we want to rely on datatable filters

        public static string BuildSql(string storageString, DataTable tableOfReference)
        {
            var parts = ReOper.Split(storageString)
                .Where(p => !ReOper.IsMatch(p))
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToArray();
            var operators = ReOper.Matches(storageString);

            if (parts.Length != operators.Count + 1)
                throw new Exception("Invalid sytnax");

            var result = new StringBuilder();

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                var v = Re.Match(part);
                if (v.Success)
                {
                    var rv = new MvdPropertyRuleValue(
                        v.Groups["varDI"].Value,
                        v.Groups["varVal"].Value,
                        v.Groups["cmpRule"].Value
                        );
                    result.Append(rv.ToSql(tableOfReference));

                    if (i < operators.Count)
                    {
                        result.Append(operators[i].Value.ToUpperInvariant());
                    }
                }
            }

            return result.ToString();
        }

        private string ToSql(DataTable tableOfReference)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0} {1} {2}", DataIndicator.ColumnName, DataComparison, DataValue);

            // equal and different can work on any comparison
            if (DataComparison == "=" ||
                DataComparison == "!=")
            {
                return sb.ToString();
            }

            // from here on it's only >, >=, <, <= 
            // (unless invalid which is thrown at the end)

            // if not string no need to convert
            if (tableOfReference.Columns[DataIndicator.ColumnName].DataType != typeof(string))
                return sb.ToString();

            sb = new StringBuilder();

            if (DataComparison == "<" ||
                DataComparison == "<=" ||
                DataComparison == ">=" ||
                DataComparison == ">")
            {
                sb.AppendFormat("convert({0}, 'System.Double') {1} {2}", DataIndicator.ColumnName, DataComparison, DataValue);
                return sb.ToString();
            }

            // otherwise invalid
            throw  new InvalidDataException();

        }
    }
}
