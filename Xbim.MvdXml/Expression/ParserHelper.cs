using System.Collections.Generic;

namespace Xbim.MvdXml.Expression
{
    internal partial class Parser
    {
        private readonly QueryingEnvironment _environment;

        internal Parser(Scanner lex, QueryingEnvironment environment) : base(lex)
        {
            _environment = environment;
        }

        private void Following(string val)
        {
            //var f = new FollowStep(val);
            //if (_environment == null)
            //    return;
            //_environment.StepsOfEntitySelection.Add(f);
            //// set the action queue
            //_environment.StepsSequence.Add(QueryStepType.Following);
        }
        
        private void Selecting()
        {
            //// prepare data
            //var sStep = new SelectionStep(_thisLevelVariables.ToArray());
            //if (_environment == null)
            //    return;
            //_environment.StepsOfFieldSelection.Add(sStep);
            //_thisLevelVariables.Clear();
            //// set the action queue
            //_environment.StepsSequence.Add(QueryStepType.Selecting);    
        }

        private void Filtering()
        {
            //// prepare data for the action
            //var sStep = new FilterStep() {Conditions = _thisLevelFilters};
            //if (_environment == null)
            //    return;
            //_environment.StepsOfFiltering.Add(sStep);
            
            //// clear the preparation buffer
            //_thisLevelVariables.Clear();

            //// set the action queue
            //_environment.StepsSequence.Add(QueryStepType.Filtering);
        }

        readonly List<string> _thisLevelVariables = new List<string>();

        /// <summary>
        /// Functions that prepares the selection variable list from the parser
        /// </summary>
        /// <param name="varName">the variable (with namespace) to be sought in the cacheRepository</param>
        /// <param name="varAlias">the alias assigned to the variable in the returning array</param>
        private void SetVar(string varName, string varAlias = "")
        {
            //if (varAlias == "")
            //    varAlias = varName;
            //if (_environment == null)
            //    return;
            //if (!_environment.DicVariables.ContainsKey(varAlias))
            //{
            //    // _environment.StepsOfFieldSelection.Count provides an index to retrieve the temorary storage 
            //    // this is to be used for filtering
            //    var vr = new SelectionRequest(varName, varAlias, _environment.StepsOfFieldSelection.Count);
            //    _environment.DicVariables.Add(varAlias, vr);
            //}
            //_thisLevelVariables.Add(varAlias);
        }

        readonly List<FilterCondition> _thisLevelFilters = new List<FilterCondition>();

        private void SetCondition(string v1, Tokens v2, string v3)
        {
            var cnd = new FilterCondition(v1, v2, v3);
            _thisLevelFilters.Add(cnd);
        }
    }
}
