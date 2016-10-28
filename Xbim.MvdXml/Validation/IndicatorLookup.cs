using System.Collections.Generic;

namespace Xbim.MvdXml.Validation
{
    // todo: this class need renaming and documentation
    public class IndicatorLookup
    {
        private readonly Dictionary<string, List<Indicator.ValueSelectorEnum>> _fastIndicators;

        public IndicatorLookup(IEnumerable<Indicator> dataIndicators)
        {
            _fastIndicators = new Dictionary<string, List<Indicator.ValueSelectorEnum>>();
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
                    _fastIndicators.Add(indicator.VariableName, new List<Indicator.ValueSelectorEnum>() { indicator.VariableValueSelector });
                }
            }
        }

        public bool Contains(string storageName)
        {
            return _fastIndicators.ContainsKey(storageName);
        }

        public bool requires(string storageName, Indicator.ValueSelectorEnum value)
        {
            return _fastIndicators.ContainsKey(storageName)
                && _fastIndicators[storageName].Contains(value);
        }
    }
}
