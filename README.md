# LoxLang

Lang in C# based on [Crafting Interpreters](https://craftinginterpreters.com/contents.html)

## Exercises Bonus
- You can try some of asked exercises by adding compile Constant
- Scan multiline-comments [CHALLENGE_SCANNING](./LoxLang.Core/LoxLang.Core.csproj)
- Check division by 0 and allow `string` + `number` concatenation [CHALLENGE_INTERPRET](./LoxLang.Core/LoxLang.Core.csproj)
- variable declaration must be initialized [CHALLENGE_STATEMENT](./LoxLang.Core/LoxLang.Core.csproj)

## Bonus Todo:
- [modulo (`%`), prefix & postfic increment/decrement (`++` `--`)](https://craftinginterpreters.com/the-lox-language.html#precedence-and-grouping)
- [`do-while` & ∞`loop`](https://craftinginterpreters.com/the-lox-language.html#control-flow)
- [expression-bodied functions](https://craftinginterpreters.com/the-lox-language.html#functions)
- [`new` keyword for class instanciation](https://craftinginterpreters.com/the-lox-language.html#classes-in-lox)
- [primitive as classes](https://craftinginterpreters.com/the-lox-language.html#inheritance)
- [implement more std library functions/classes](https://craftinginterpreters.com/the-lox-language.html#the-standard-library)
- [implement better error handling](https://craftinginterpreters.com/scanning.html#error-handling)


## Grammar
```ebnf
program        = declaration* EOF ;
declaration    = funDecl
               | varDecl
               | statement ;
statement      = funsStmt
               | forStmt
               | ifStmt
               | whileStmt
               | block ;
funsStmt       = exprStmt
               | printStmt
               | returnStmt;
returnStmt     = "return" expression? ";" ;
ifStmt         = "if" "(" expression ")" statement
               ( "else" statement )? ;
whileStmt      = "while" "(" expression ")" statement ;
forStmt        = "for" "(" ( varDecl | exprStmt | ";" )
                 expression? ";"
                 expression? ")" statement ;
block          = "{" declaration* "}" ;
funDecl        = "fun" function ;
function       = IDENTIFIER "(" parameters? ")" (block | ":" statement ) ;
lambdaExpr   = "fun" "(" parameters? ")" (block | ":" statement ) ;
parameters     = IDENTIFIER ( "," IDENTIFIER )* ;
varDecl        = "var" IDENTIFIER ( "=" expression )? ";" ;
exprStmt       = expression ";" ;
printStmt      = "print" expression ";" ;
expression     = assignment ;
assignment     = IDENTIFIER "=" assignment
               | logicalOr ;
logicalOr      = logicand ( "or" logicAnd )* ;
logicalAnd     = equality ( "and" equality )* ;
equality       = comparison ( ( "!=" | "==" ) comparison )* ;
comparison     = term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
term           = factor ( ( "-" | "+" ) factor )* ;
factor         = unary ( ( "/" | "*" ) unary )* ;
unary          = ( "!" | "-" ) unary
               | call ;
call           = primary ( "(" arguments? ")" )* ;
arguments      = expression ( "," expression )* ;
primary        = NUMBER | STRING
               | "true" | "false" | "nil"
               | "(" expression ")"
               | lambdaExpr
               | IDENTIFIER ;
```
