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
%token OP_AND
%token OP_OR
%token OP_XOR

/* Keywords  */
%token OP_EQUAL 				
%token OP_NOT_EQUAL 				
%token OP_GREATER_THAN 			
%token OP_GREATER_THAN_OR_EQUAL 	
%token OP_LESS_THAN 				
%token OP_LESS_THAN_OR_EQUAL

/* Others */

%token METRICTEXT
%token SQBR_OPEN
%token SQBR_CLOSE
%token DOUBLEQUOTE

%%
expression
	: booleanExpressions
	;

booleanExpressions
	: booleanExpressions logical_interconnection booleanExpression
	| booleanExpression 
	;

booleanExpression
	: evaluable operator evaluable
	| '(' booleanExpressions ')';
evaluable
	: parameter 
	| parameter metric
	| value;

parameter
	: STRING;

metric
	: SQBR_OPEN METRICTEXT SQBR_CLOSE;

value
	: FALSE 	| TRUE	| UNKNOWN	| INTEGER	| DOUBLE 	| DOUBLEQUOTE STRING DOUBLEQUOTE;
	
logical_interconnection
	: OP_AND 	| OP_OR 	| OP_XOR;	
operator
	: OP_EQUAL								{$$.val = Tokens.OP_EQUAL;}	| OP_NOT_EQUAL 							{$$.val = Tokens.OP_NOT_EQUAL;}	| OP_GREATER_THAN 						{$$.val = Tokens.OP_GREATER_THAN;}	| OP_GREATER_THAN_OR_EQUAL 				{$$.val = Tokens.OP_GREATER_THAN_OR_EQUAL;}	| OP_LESS_THAN 							{$$.val = Tokens.OP_LESS_THAN;}	| OP_LESS_THAN_OR_EQUAL;				{$$.val = Tokens.OP_LESS_THAN_OR_EQUAL;}	;
	
%%