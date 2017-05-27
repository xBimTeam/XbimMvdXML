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
        public MetricEnum VariableValueSelector;

        /// <summary>
        /// Sepcifies the value of interest of an identifier.
        /// See buildingSMART - mvdMXL v1.1 specification document, page 30.
        /// </summary>
        public enum MetricEnum
        {
            /// <summary>
            /// this can be tested to see if the parsing was successful
            /// </summary>
            Error, // 
            /// <summary>
            /// Indicates the value of the attribute 
            /// </summary>
            Value,
            /// <summary>
            /// Indicates the type of the value assigned to the attribute (STRING). 
            /// </summary>
            Type,
            /// <summary>
            /// Boolean value returning (Value != null).
            /// Not formally defined in the documentation.
            /// </summary>
            Exists,
            /// <summary>
            /// Indicates the size of a collection or STRING (INTEGER). 
            /// </summary>
            Size
        }

        internal DataIndicator(string parameter, string metricValue)
        {
            VariableName = parameter;
            if (!Enum.TryParse(metricValue, out VariableValueSelector))
                VariableValueSelector = MetricEnum.Error;
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
                        VariableValueSelector = MetricEnum.Error;
                    return;
                }
                VariableValueSelector = MetricEnum.Value;
                return;
            }
            VariableName = "Undefined";
            VariableValueSelector = MetricEnum.Error;
        }

        public string ColumnName => GetColumnName(VariableName, VariableValueSelector);

        internal static string GetColumnName(string variableName, MetricEnum variableSelector)
        {
            if (variableSelector == MetricEnum.Value)
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
