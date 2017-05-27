%{
	
%}
%namespace Xbim.MvdXml.Expression
%partial   
%parsertype Parser
%output=Parser.cs
%visibility internal
%using System.Linq.Expressions
%using Xbim.MvdXml.DataManagement

%start boolean_expression

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

/* logical_interconnection */
%token LI_AND
%token LI_OR
%token LI_XOR  
%token LI_NAND  
%token LI_NOR  
%token LI_NXOR 

/* operators */

%token  SQBR_LEFT
%token  SQBR_RIGHT

%%

boolean_expression
	: boolean_expression logical_interconnection boolean_term
	| boolean_term	
	;

boolean_term	
	: leftTerm op_compare rightTerm					{SetCondition($1, ((Tokens)($2.val)), $3);}
	| SQBR_LEFT boolean_expression SQBR_RIGHT		{SetCondition($1, ((Tokens)($2.val)), $3);}
	;

leftTerm
	: ID											{$$.val = new DataIndicator($1.strVal, "");} 
	| ID metric										{$$.val = new DataIndicator($1.strVal, $2.strVal);}
	| metric										{$$.val = new DataIndicator("", $1.strVal);}
	;

metric 
	: SQBR_LEFT ID SQBR_RIGHT;


rightTerm
	: ID
	| ID metric
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

logical_interconnection
	: LI_AND			{$$.val = Tokens.LI_AND;}
    | LI_OR				{$$.val = Tokens.LI_OR;}
    | LI_XOR			{$$.val = Tokens.LI_XOR;}
    | LI_NAND			{$$.val = Tokens.LI_NAND;}
	| LI_NOR			{$$.val = Tokens.LI_NOR;}
	| LI_NXOR			{$$.val = Tokens.LI_NXOR;}
	;
	
%%
