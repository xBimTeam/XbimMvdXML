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

/* ********************** Operators ************************** */

"="		{  return ((int)Tokens.OP_EQ); }
"!="	{ return ((int)Tokens.OP_NEQ); }
">"		{  return ((int)Tokens.OP_GT); }
"<"		{  return ((int)Tokens.OP_LT); }
">=" 	{  return ((int)Tokens.OP_GTE); }
"<=" 	{  return ((int)Tokens.OP_LTQ); }
"&"		{  return ((int)Tokens.OP_AND); }
"|"		{  return ((int)Tokens.OP_OR); }


/* ********************** Operators ************************** */

"\133"		{  return ((int)Tokens.SQBR_LEFT); }
"\135"		{  return ((int)Tokens.SQBR_RIGHT); }


					 

/* ********************     components        ****************** */
// when adding items here you need to consider the behaviour of the ScannerHelper.SetValue() 

[a-z]+[a-z0-9_]*		{ return (int)SetValue(Tokens.ID); }
[\-\+]?[0-9]+	    {  return (int)SetValue(Tokens.INTEGER); }
[\-\+]?[0-9]*[\.][0-9]*	|
[\-\+\.0-9][\.0-9]+E[\-\+0-9][0-9]* { return (int)SetValue(Tokens.DOUBLE); }
[\047]+([\040-\046\050-\176\200-\377]|[\047][\047])*[\047]+ { return (int)SetValue(Tokens.STRING); }

// when adding items here you need to consider the behaviour of the ScannerHelper.SetValue() 


/* -----------------------  Epilog ------------------- */
%{
	yylloc = new LexLocation(tokLin,tokCol,tokELin,tokECol);
%}
/* --------------------------------------------------- */
%%


