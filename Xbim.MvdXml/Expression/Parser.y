%{
	
%}
%namespace Xbim.MvdXml.Expression
%partial   
%parsertype Parser
%output=Parser.cs
%visibility internal
%using System.Linq.Expressions

%start expression

%union{
		public string strVal;
		public int intVal;
		public double doubleVal;
		public bool boolVal;
		public Type typeVal;
		public object val;
	  }


%token	INTEGER	
%token	DOUBLE	
%token	STRING	
%token FALSE
%token TRUE
%token UNKNOWN

/* operators */
%token  OP_AND
%token  OP_OR
%token  OP_XOR

/* comp operators  */
%token  OP_LESS_THAN_OR_EQUAL

/* Keywords  */
%token AS_NAME
%token OP_NOT_EQUAL 				
%token OP_GREATER_THAN 			
%token OP_GREATER_THAN_OR_EQUAL 	
%token OP_LESS_THAN 				
%token OP_LESS_THAN_OR_EQUAL

/* Others */

%token METRICTEXT

%%
expression
	: booleanExpression
	;

booleanExpression
	: booleanExpression logical_interconnection boolean_term
	| boolean_term 
	;

boolean_term
	: evaluable operator evaluable
	| '(' boolean_expression ')';
evaluable
	: parameter 
	| parameter metric
	| value;

metric
	: SQBR_OPEN METRICTEXT SQBR_CLOSE;

value
	: FALSE 	| TRUE	| UNKNOWN	| INTEGER	| DOUBLE 	| STRING;
	
logical_interconnection
	: OP_AND 	| OP_OR 	| OP_XOR;	
operator
	: OP_EQUAL								{$$.val = Tokens.OP_EQUAL;}	| OP_NOT_EQUAL 							{$$.val = Tokens.OP_NOT_EQUAL;}	| OP_GREATER_THAN 						{$$.val = Tokens.OP_GREATER_THAN;}	| OP_GREATER_THAN_OR_EQUAL 				{$$.val = Tokens.OP_GREATER_THAN_OR_EQUAL;}	| OP_LESS_THAN 							{$$.val = Tokens.OP_LESS_THAN;}	| OP_LESS_THAN_OR_EQUAL;				{$$.val = Tokens.OP_LESS_THAN_OR_EQUAL;}	;
	
%%
