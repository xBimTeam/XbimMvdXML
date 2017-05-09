using System.Collections.Generic;

namespace QUT.Xbim.Gppg
{
    internal enum NullEnum
    {   
    }
}

namespace Xbim.MvdXml.Expression
{
    internal partial class Parser
    {
        private readonly QueryingEnvironment _environment;

        internal Parser(Scanner lex, QueryingEnvironment environment) : base(lex)
        {
            _environment = environment;
        }

        readonly List<FilterCondition> _thisLevelFilters = new List<FilterCondition>();

        private void SetCondition(object v1, Tokens v2, object v3)
        {
            
            
        }
    }
}
