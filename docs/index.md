# Nutmeg in a Nutshell

**>>>>>> Work in Progress <<<<<<**

Nutmeg is a teaching-oriented imperative programming language that aims to provide seamless integration of the individual features of functional programming. This includes the ability to write higher-order functions, as do most modern programming languages. However, Nutmeg goes further and supports abilities such as:

- Declaring functions side-effect free and/or to only work on immutable values.
- Both mutable and immutable versions for all built-in classes.
- Copy-on-write updating of objects.
- Deferring individual expressions to be evaluated lazily and/or declaring function bodies to use lazy evalation throughout.
- 'Locking' values against change temporarily or permanently.

## Temporary list of contents

While we are still developing Nutmeg, this guide is very far from complete. However, here are a few in-progress documents you may find useful.

* [Tokenisation Rules as EBNF Grammar](Tokens.ebnf.txt) and as a [railroad diagram](Tokens.pdf).
* [Nutmeg Syntax as EBNF Grammar](Nutmeg.ebnf.txt) and as a [railroad diagram](Nutmeg-prototype-grammar.pdf).
* [Assignments](assignments.md) - a page on assignment in Nutmeg.
* [Comments](comments.md) - a page on comments in Nutmeg.
* [Compiling and Running Programs](compiling-and-running.md) - a page on how to use the nutmeg and nutmegc commands.
* [For syntax](for-syntax.md) - a page on of `for` loops i Nutmeg.
* [If Syntax](if-syntax.md) - a page on the `if` conditional in Nutmeg.
* [Ranges](ranges.md) - a page on ranges in Nutmeg.
* [Statements](statements.md) - a page on the syntax of statements.
* [Unit tests](unittest.md) - a page on the built-in unit-test framework.
* [Variables](variables.md) - a page on the different kinds of variables.

