using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Xbim.MvdXml.Expression
{
    internal sealed partial class Scanner
    {
        /// <summary>
        /// String processing funtion used during the scanning.
        /// </summary>
        /// <returns>Token PRODUCT, PRODUCT_TYPE or STRING</returns>
        private Tokens ProcessString()
        {
            yylval.strVal = yytext;
           

            //return string otherwise
            return Tokens.STRING;
        }

        /// <summary>
        /// function used by scanner to set values for value type tokens
        /// </summary>
        /// <param name="type">Value type. If no value type is specified 'STRING' is used by default</param>
        /// <returns>Token set by the function</returns>
        private Tokens SetValue(Tokens type = Tokens.STRING)
        {
            yylval.strVal = yytext;

            switch (type)
            {
                case Tokens.INTEGER:
                    if (int.TryParse(yytext, out yylval.intVal))
                        return type;
                    break;
                case Tokens.DOUBLE:
                    try
                    {
                        yylval.doubleVal = double.Parse(yytext, CultureInfo.InvariantCulture);
                        return type;
                    }
                    catch (Exception)
                    {
                        
                    }
                    break;
                default:
                    if (yytext.StartsWith("'") && yytext.EndsWith("'"))
                    {
                        var val = yytext.Substring(1, yytext.Length - 2);
                        val = val.Replace(@"''", @"'");
                        yylval.strVal = val;
                    }
                    else if (yytext.StartsWith("\"") && yytext.EndsWith("\""))
                    {
                        var val = yytext.Substring(1, yytext.Length - 2);
                        val = val.Replace("\"\"", "\"");
                        yylval.strVal = val;
                    }    
                    else
                    {
                        yylval.strVal = yytext;
                    }
                    return Tokens.STRING;
            }
            return Tokens.STRING;
        }


        /// <summary>
        /// Creates dictionary where keys are variants of product names and values are the types
        /// </summary>
        /// <returns>Dictionary of variants of the product names</returns>
        private static Dictionary<string, Type> GetNames(Type baseType)
        {
            var assembly = baseType.Assembly;
            var types = assembly.GetTypes().Where(t => baseType.IsAssignableFrom(t));
            var result = new Dictionary<string, Type>();
            foreach (var type in types)
            {
                string name = type.Name;
                //plain IFC name
                result.Add(name.ToLower(), type);

                //IFC name without Ifc prefix
                string shortName = name.Remove(0, 3);
                result.Add(shortName.ToLower(), type);

                //IFC name without Ifc prefix and splitted according to camel case.
                string splitName = SplitCamelCase(name.Remove(0, 3));
                if (splitName != shortName)
                    result.Add(splitName.ToLower(), type);
            }
            return result;
        }

        /// <summary>
        /// Splits the string from camel case to underscore separated strings
        /// </summary>
        /// <param name="input">Input string</param>
        /// <returns>Splitted camel-case string</returns>
        private static string SplitCamelCase(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", "_$1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim('_');
        }

        /// <summary>
        /// List of errors
        /// </summary>
        public List<string> Errors = new List<string>();

        /// <summary>
        /// List of error locations
        /// </summary>
        public List<ErrorLocation> ErrorLocations = new List<ErrorLocation>();

        /// <summary>
        /// Overriden yyerror function for error reporting
        /// </summary>
        /// <param name="format">Formated error message</param>
        /// <param name="args">Error arguments</param>
        public override void yyerror(string format, params object[] args)
        {
            Errors.Add(String.Format(format, args) + String.Format("From line {0}, column {1} to line {2}, column {3}", yylloc.StartLine, yylloc.StartColumn, yylloc.EndLine, yylloc.EndColumn));
            ErrorLocations.Add(new ErrorLocation() { 
                StartLine = yylloc.StartLine, 
                EndLine = yylloc.EndLine, 
                StartColumn = yylloc.StartColumn, 
                EndColumn = yylloc.EndColumn,
                Message = String.Format(format, args)
            });

            base.yyerror(format, args);
        }


    }

    public struct ErrorLocation
	{
		public int StartLine;
        public int EndLine;
        public int StartColumn;
        public int EndColumn;
        public string Message;
	}
}
