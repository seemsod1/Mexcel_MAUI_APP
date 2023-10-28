grammar Counting;

/*
 * Parser Rules
 */

compileUnit : expression EOF;

expression :
    LPAREN expression RPAREN            # ParenthesizedExpr
    | expression operatorToken=(UNARY_PLUS | UNARY_MINUS) expression   # UnaryExpr
    | expression operatorToken=(MULTIPLY | DIVIDE) expression        # MultiplicativeExpr
    | expression operatorToken=EXPONENT expression   # ExponentiationExpr
    | expression operatorToken=(ADD | SUBTRACT) expression           # AdditiveExpr
    | expression operatorToken=(LESS | LESS_EQUAL | GREATER | GREATER_EQUAL | NOT_EQUAL) expression # ComparisonExpr
    | NUMBER                         # NumberExpr
    | IDENTIFIER                     # IdentifierExpr
    ;

/*
 * Lexer Rules
 */

NUMBER : INT ('.' INT)?;
IDENTIFIER : [a-zA-Z]+[1-9][0-9]*;

INT : ('0'..'9')+;

EXPONENT : '^';
MULTIPLY : '*';
DIVIDE : '/';
SUBTRACT : '-';
ADD : '+';
UNARY_PLUS : '+';
UNARY_MINUS : '-';
LPAREN : '(';
RPAREN : ')';
LESS : '<';
LESS_EQUAL : '<=';
GREATER : '>';
GREATER_EQUAL : '>=';
NOT_EQUAL : '<>';

WS : [ \t\r\n] -> channel(HIDDEN);
