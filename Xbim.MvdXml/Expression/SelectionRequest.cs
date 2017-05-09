namespace Xbim.MvdXml.Expression
{
    internal class SelectionRequest
    {
        internal string VariableName;
        internal string VariableAlias;
        internal int SelectionStepIndex;


        internal SelectionRequest(string variableName, string variableAlias, int selectionStepIndex)
        {
            VariableName = variableName;
            VariableAlias = variableAlias;
            SelectionStepIndex = selectionStepIndex;
        }
    }
}
