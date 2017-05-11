%{
	
%}
%namespace Xbim.MvdXml.Expression
%partial   
%parsertype Parser
%output=Parser.cs
%visibility internal
%using System.Linq.Expressions

%start condition

%union{
		public string strVal;
		public int intVal;
		public double doubleVal;
		public bool boolVal;
		public Type typeVal;
		public object val;
		public Tokens token;
	  }

/* basic blocs */
%token	INTEGER	
%token	DOUBLE	
%token	ID
%token	STRING	

/* operators */
%token  OP_EQ			/*is equal, equals, is, =*/
%token  OP_NEQ			/*is not equal, is not, !=*/
%token  OP_GT			/*is greater than, >*/
%token  OP_LT			/*is lower than, <*/
%token  OP_GTE			/*is greater than or equal, >=*/
%token  OP_LTQ			/*is lower than or equal, <=*/
%token  OP_LIKE			/*like, contains*/
%token  OP_AND
%token  OP_OR

/* operators */

%token  SQBR_LEFT
%token  SQBR_RIGHT

%%

condition	
	: leftTerm op_compare rightTerm					{SetCondition($1, ((Tokens)($2.val)), $3);}
	;

leftTerm
	: ID
	| ID metric
	;

metric 	: SQBR_LEFT ID SQBR_RIGHT;

rightTerm
	: ID
	| DOUBLE
	| INTEGER
	| STRING;


op_compare
	: OP_GT			{$$.val = Tokens.OP_GT;}
    | OP_LT			{$$.val = Tokens.OP_LT;}
    | OP_GTE		{$$.val = Tokens.OP_GTE;}
    | OP_LTQ		{$$.val = Tokens.OP_LTQ;}
	| OP_EQ			{$$.val = Tokens.OP_EQ;}
	| OP_NEQ		{$$.val = Tokens.OP_NEQ;}
	| OP_LIKE		{$$.val = Tokens.OP_LIKE;}
	;
	
%%
