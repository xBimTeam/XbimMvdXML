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

        private static readonly Regex Re = new Regex(@" *(?<varDI>.+?) *(?<cmpRule>[!=\<\>])+ *(?<varVal>.*) *");

        internal static IEnumerable<MvdPropertyRuleValue> GetValues(string storageString)
        {
            var vals = new List<MvdPropertyRuleValue>();
            var parts = storageString.Split(new[] { " And ", " and ", " AND ", ";" }, StringSplitOptions.RemoveEmptyEntries);
           
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
            var pars = GetValues(storageString);
            var lst = pars.Select(mvdPropertyRuleValue => mvdPropertyRuleValue.ToSql(tableOfReference)).ToList();
            return string.Join(" AND ", lst);
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
