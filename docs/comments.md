# Comments

## Overview

Like most programming languages, Nutmeg has two kinds of comments, end of line comments and long comments. End of line comments are introduced with three (or more) hash `#` marks. e.g.

```
### This is an end of line comment.
################################### so is this.
```

Long comments start with two hashes and a parenthesis `##(`. The long comment is closed by two hashes and a matching close parenthesis e.g. `##)`. Unlike languages such as C, long comments nest.

```
##( 
This is a good way to introduce a long explanatory comment
that covers several lines. You can also use it to temporarily 
comment out large sections of code (try to remove it before 
checking in your code).
##)
```

## Additional Technical Remarks

Comments are text regions that have no tokens but they do act as a break between tokens.  Now there is a subtle aspect to comments in Nutmeg. Because Nutmeg uses end-of-lines to substitute for semi-colons, it matters whether a comment is counted as an end-of-line. The rule is this:

* If the comment was an end of line comment or a long-comment that included an end of line then the comment acts as a end-of-line break.

This rule means that comments do not suddenly and unexpectedly force the use of semi-colons in the preceeding code. For example:
```
def foo( x ):
    y := x + 1    ### No semi-colon required
    y * y
enddef
```