using System.Collections.Generic;
using System.Text;

namespace Xbim.MvdXml.DataManagement
{
    // todo: this class could need renaming and documentation
    public class DataIndicatorLookup
    {
        private readonly Dictionary<string, List<DataIndicator.ValueSelectorEnum>> _fastIndicators;

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var fastIndicator in _fastIndicators)
            {
                sb.Append(fastIndicator.Key + ":");
                sb.Append(string.Join(",", fastIndicator.Value.ToArray()));
                sb.Append(";");
            }
            return sb.ToString();
        }

        public DataIndicatorLookup(IEnumerable<DataIndicator> dataIndicators)
        {
            _fastIndicators = new Dictionary<string, List<DataIndicator.ValueSelectorEnum>>();
            foreach (var indicator in dataIndicators)
            {
                if (_fastIndicators.ContainsKey(indicator.VariableName))
                {
                    // no duplicates in list
                    if (!_fastIndicators[indicator.VariableName].Contains(indicator.VariableValueSelector))
                        _fastIndicators[indicator.VariableName].Add(indicator.VariableValueSelector);
                }
                else
                {
                    _fastIndicators.Add(indicator.VariableName, new List<DataIndicator.ValueSelectorEnum>() { indicator.VariableValueSelector });
                }
            }
        }

        /// <summary>
        /// Creates a list of indicators with value only access
        /// </summary>
        /// <param name="values"></param>
        public DataIndicatorLookup(params string[] values)
        {
            _fastIndicators = new Dictionary<string, List<DataIndicator.ValueSelectorEnum>>();
            foreach (var value in values)
            {
                _fastIndicators.Add(value, new List<DataIndicator.ValueSelectorEnum>() {DataIndicator.ValueSelectorEnum.Value });
            }
        }

        public bool Contains(string storageName)
        {
            return _fastIndicators.ContainsKey(storageName);
        }

        public bool Requires(string storageName, DataIndicator.ValueSelectorEnum value)
        {
            return _fastIndicators.ContainsKey(storageName)
                && _fastIndicators[storageName].Contains(value);
        }
    }
}
