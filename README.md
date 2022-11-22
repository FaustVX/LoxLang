# LoxLang

Lang in C# based on [Crafting Interpreters](https://craftinginterpreters.com/contents.html)

## Bonus Todo:
- [modulo (`%`), prefix & postfic increment/decrement (`++` `--`)](https://craftinginterpreters.com/the-lox-language.html#precedence-and-grouping)
- [`do-while` & ∞`loop`](https://craftinginterpreters.com/the-lox-language.html#control-flow)
- [expression-bodied functions](https://craftinginterpreters.com/the-lox-language.html#functions)
- [`new` keyword for class instanciation](https://craftinginterpreters.com/the-lox-language.html#classes-in-lox)
- [primitive as classes](https://craftinginterpreters.com/the-lox-language.html#inheritance)
- [implement more std library functions/classes](https://craftinginterpreters.com/the-lox-language.html#the-standard-library)
- [implement better error handling](https://craftinginterpreters.com/scanning.html#error-handling)

## Grammar
### Global
```ebnf
program        = declaration* EOF ;
declaration    = funDecl
               | classDecl
               | varDecl
               | statement ;
statement      = funsStmt
               | forStmt
               | ifStmt
               | whileStmt
               | loopStmt
               | breakStmt
               | block ;
expression     = assignment ;
```
### Declarations
```ebnf
funDecl        = "fun" function ;
classDecl      = "class" IDENTIFIER ( ":" IDENTIFIER )?
                 "{" ( "class"? ( function | getter ))* "}" ;
varDecl        = "var" IDENTIFIER ( "=" expression )? ";" ;
```
### Statements
```ebnf
funsStmt       = exprStmt
               | printStmt
               | returnStmt;
forStmt        = "for" "(" ( varDecl | exprStmt | ";" )
                 expression? ";"
                 expression? ")" statement ;
ifStmt         = "if" "(" expression ")" statement
               ( "else" statement )? ;
whileStmt      = "while" "(" expression ")" statement ;
loopStmt      = "loop" statement ;
breakStmt      = "break" ";" ;
block          = "{" declaration* "}" ;
exprStmt       = expression ";" ;
printStmt      = "print" expression ";" ;
returnStmt     = "return" expression? ";" ;
```
### Expressions
```ebnf
assignment     = ( call "." )? IDENTIFIER ( "=" | "+=" | "-=" | "*=" | "/=" | "%=" ) assignment
               | logicalOr ;
logicalOr      = logicalAnd ( "or" logicalAnd )* ;
logicalAnd     = equality ( "and" equality )* ;
equality       = comparison ( ( "!=" | "==" ) comparison )* ;
comparison     = term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
term           = factor ( ( "-" | "+" ) factor )* ;
factor         = unary ( ( "/" | "*" | "%" ) unary )* ;
unary          = ( "!" | "-" ) unary
               | ( "++" | "--" ) ( call "." )? IDENTIFIER
               | call ;
call           = primary ( "(" arguments? ")" | "." IDENTIFIER )* ;
primary        = NUMBER | STRING
               | "true" | "false" | "nil"
               | "(" expression ")"
               | lambdaExpr
               | IDENTIFIER
               | "super" "." IDENTIFIER ;
lambdaExpr     = "fun" "(" parameters? ")" (block | ":" expression ) ;
```
### Others
```ebnf
arguments      = expression ( "," expression )* ;
function       = IDENTIFIER "(" parameters? ")" (block | ":" exprStmt ) ;
getter         = IDENTIFIER (block | ":" exprStmt ) ;
parameters     = IDENTIFIER ( "," IDENTIFIER )* ;
```
