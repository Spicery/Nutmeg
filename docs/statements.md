# Statements

## Overview

In Nutmeg, you may have noticed that the majority of control constructs have paired opening and closing keywords, with different parts separated by 'punctuation' keywords. For example `for`-`do`-`endfor` and `def`-`enddef`. This means that statements can only occur when safely bracketed between a start and end keyword. So when we put statements in a for loop, they are sandwiched between the `do` and `endfor` keywords.
```
for i in [0 ..< 10] do
    val k := ( i + 1 ) ** 2
    accumulate( k )
endfor 
```

This design is deliberate because it means that anywhere that a statement can occur, you can use multiple statements without any additional syntax. So, unlike languages like Javascript, you don't need braces to string multiple statements together. 

Individual statements are separated either by semicolons or by line-breaks. Strictly speaking, Nutmeg _could_ tell different statements apart without the help of semicolons or newlines. But experience has shown that this little bit of syntactic redundancy makes it easier for the compiler to help coders by spotting typos and other elementary mistakes, which is very helpful for practical programming. 

Line breaks are the preferred way of separating statements, simply because there's less visual clutter. Of course, you might actually prefer semicolons, and that's fine too. The only times that it is necessary to use use semi-colons is when you put multiple statements on one line. This is really unusual - but it is there if you need it. 
```
var x = y; x <- x + 1; y <- y - 1;   ### All on one line
```

Using line-breaks is visually very neat but it can get in the way when we need to write a large expression that is best spread over several lines for readability. So we need a way of ignoring line-breaks temporarily. Nutmeg does this by ignoring line-breaks between opening and closing keywords. To cope with the fact that statements can be nested in expression-brackets and expression-brackets within statements, Nutmeg uses the following rules:

* At the start of a statement-context, enable the line-break setting
* At the end of a statement-context, restore line-breaks setting to its previous value
* At the opening keyword of an expression 'bracket', disable the line-break setting
* At the ending keyword of an expression 'bracket', restore line-breaks setting to its previous value

Fortunately, with conventional indentation, it is visually easy to spot where line breaks are being used to separate statements.

## Technical Summary

Statements in Nutmeg always appear in a syntactic context called a statement-sequence that allows multiple statements. Individual statements are terminated by either a semi-colon or a new-line.  Here's some places where statement-sequences appear in Nutmeg, where `E` stands for expression and `S` for a statement sequence.
```
### Conditionals
if E then S elseif E then S else S endif

### Loops
for i in E do S endfor

### Definitions
def F =>> S enddef
```

Within a statement-sequence newlines are signficant. To allow a statement to span several lines, newline-termination is disabled between expression brackets (e.g. `( ... )`, `if ... endif`, `for ... endfor`) and re-enabled by any embedded statement-sequence. So newline-termination is determined by the narrowest enclosing bracket-context or statement-sequence context.
