%namespace Xbim.MvdXml.Expression

%option verbose, summary, caseinsensitive, noPersistBuffer, out:Scanner.cs
%visibility internal

%{
	//all the user code is in XbimQueryScanerHelper

%}
 
%%

%{
		
%}
/* ************  skip white chars and line comments ************** */
"\t"	     {}
" "		     {}
[\n]		 {} 
[\r]         {} 
[\0]+		 {} 
\/\/[^\r\n]* {}   /*One line comment*/

/* ********************** Operators ************************** */

"="	|
"equals" |
"is" |
"is equal to"					{  return ((int)Tokens.OP_EQUAL); }

"!=" |
"is not equal to" |
"is not" |
"does not equal" |
"doesn't equal"					{ return ((int)Tokens.OP_NOT_EQUAL); }

">" |
"is greater than"				{  return ((int)Tokens.OP_GREATER_THAN); }

"<"	|
"is less than"					{  return ((int)Tokens.OP_LESS_THAN); }

">=" |
"is greater than or equal to"	{  return ((int)Tokens.OP_GREATER_THAN_OR_EQUAL); }

"<=" |
"is less than or equal to"		{  return ((int)Tokens.OP_LESS_THAN_OR_EQUAL); }

"&" |
";" |
"and"							{  return ((int)Tokens.OP_AND); }

"|" |
"or"							{  return ((int)Tokens.OP_OR); }

"|" |
"xor"							{  return ((int)Tokens.OP_OR); }

"value" |
"type" |
"size" |
"unique"							{  return ((int)Tokens.METRICTEXT); }

METRICTEXT

"("		{  return ('('); }
")"		{  return (')'); }
"."		{  return ('.'); }
","		{  return (','); }
"_"		{  return ('_'); }

"["		{  return ((int)Tokens.SQBR_OPEN); }
"]"		{  return ((int)Tokens.SQBR_CLOSE); }

/* ********************     values        ****************** */
[\-\+]?[0-9]+	    {  return (int)SetValue(Tokens.INTEGER); }
[\-\+]?[0-9]*[\.][0-9]*	|
[\-\+\.0-9][\.0-9]+E[\-\+0-9][0-9]* { return (int)SetValue(Tokens.DOUBLE); }
[\"]([\000\011-\041\043-\176\200-\377]|[\042][\042])*[\"] |
[a-z]+[a-z0-9_\.]*		{ return (int)SetValue(Tokens.STRING); }

/* -----------------------  Epilog ------------------- */
%{
	yylloc = new LexLocation(tokLin,tokCol,tokELin,tokECol);
%}
/* --------------------------------------------------- */
%%


