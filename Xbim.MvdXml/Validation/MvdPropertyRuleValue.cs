using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Xbim.MvdXml.Validation
{
    public class MvdPropertyRuleValue
    { 
        public Indicator DataIndicator;
        public string DataValue;
        public string DataComparison;

        public MvdPropertyRuleValue(string varDataIndicator, string varDataValue, string varDataComparison)
        {
            DataIndicator = new Indicator(varDataIndicator);
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
            var parts = storageString.Split(new[] { " AND ", ";" }, StringSplitOptions.RemoveEmptyEntries);
           
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

        public static string BuildSql(string storageString)
        {
            var pars = GetValues(storageString);
            var lst = pars.Select(mvdPropertyRuleValue => mvdPropertyRuleValue.ToSql()).ToList();
            return string.Join(" AND ", lst);
        }

        private string ToSql()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0} {1} {2}", DataIndicator.ColumnName, DataComparison, DataValue);
            return sb.ToString();
        }
    }
}
