namespace Xbim.MvdXml.Expression
{
    internal class FilterCondition
    {
        internal string VariableName;
        internal Tokens Condition;       
        internal string Value;

        public FilterCondition(string variableName, Tokens condition, string value)
        {
            VariableName = variableName;
            Condition = condition;
            Value = value;
        }
    }
}
