# LoxLang

Lang in C# based on [Crafting Interpreters](https://craftinginterpreters.com/contents.html)

## Exercises Bonus
- You can try some of asked exercises by adding compile Constant
- Scan multiline-comments [CHALLENGE_SCANNING](./LoxLang.Core/LoxLang.Core.csproj)

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
expression     = literal
               | unary
               | binary
               | grouping ;

literal        = NUMBER | STRING | "true" | "false" | "nil" ;
grouping       = "(" expression ")" ;
unary          = ( "-" | "!" ) expression ;
binary         = expression operator expression ;
operator       = "==" | "!=" | "<" | "<=" | ">" | ">="
               | "+"  | "-"  | "*" | "/" ;
```
