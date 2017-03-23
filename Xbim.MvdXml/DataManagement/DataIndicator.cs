using System;
using System.Text.RegularExpressions;

namespace Xbim.MvdXml.DataManagement
{
    // todo: this class need renaming and documentation
    public class DataIndicator : IEquatable<DataIndicator>
    {
        public override string ToString()
        {
            return ColumnName;
        }

        private static readonly Regex NameAndSelectorRegex = new Regex(@"(?<varName>[^\[]+)(\[(?<valSelector>.+?)\])?");

        public string VariableName;
        public ValueSelectorEnum VariableValueSelector;

        public enum ValueSelectorEnum
        {
            Error, // this can be tested to see if the parsing was successful
            Value,
            Type,
            Exists,
            Size
        }

        public DataIndicator(string stringValue)
        {
            var v = NameAndSelectorRegex.Match(stringValue.Trim());
            if (v.Success)
            {
                VariableName = v.Groups["varName"].Value;
                if (!string.IsNullOrEmpty(v.Groups["valSelector"].Value))
                {
                    if (!Enum.TryParse(v.Groups["valSelector"].Value, out VariableValueSelector))
                        VariableValueSelector = ValueSelectorEnum.Error;
                    return;
                }
                VariableValueSelector = ValueSelectorEnum.Value;
                return;
            }
            VariableName = "Undefined";
            VariableValueSelector = ValueSelectorEnum.Error;
        }

        public string ColumnName => GetColumnName(VariableName, VariableValueSelector);

        internal static string GetColumnName(string variableName, ValueSelectorEnum variableSelector)
        {
            if (variableSelector == ValueSelectorEnum.Value)
                return variableName;
            return variableName + "__" + variableSelector;
            // return variableName + "." + variableSelector; // fails at visualisation
            // return variableName + "[" + variableSelector + "]"; // fails in data.Select()
        }

        // implemented with guidance from https://msdn.microsoft.com/en-us/library/vstudio/bb348436(v=vs.100).aspx

        public bool Equals(DataIndicator other)
        {
            //Check whether the compared object is null.
            if (ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data.
            if (ReferenceEquals(this, other)) return true;

            //Check whether the products' properties are equal.
            return VariableName.Equals(other.VariableName) &&
                   VariableValueSelector.Equals(other.VariableValueSelector);
        }

        public override int GetHashCode()
        {
            return ColumnName.GetHashCode();
        }
    }
}
