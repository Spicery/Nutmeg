# showMe

> `showMe{OPTIONS}( ITEMS... ) -> ()`
> - label=_label_ 
> - sep=_separator_
> - term=_terminator_

## Summary

This function is intended as a quick and dirty way to inspect values that is helpful to programmers. This contrasts with [println](println) that is used to prepare formatted output.

It will send a programmer-friendly representation of the arguments to the standard-output. The argument-outputs are separated by _separator_ and terminated by _terminator_.

## Named-Optional Arguments

* `label=`_label_: a prefix for the arguments. When not empty it is immediately followed by a _separator_. Defaults to the empty string.
* `sep=`_separator_: a string that will be printed between arguments to visually separate them. Defaults to a single space.
* `term=`_terminator_: a string that will be printed after the last argument. Defaults to the newline appropriate for the current system.

## Examples

```
>>> showMe{label="Values:"}( `a`, `b`, `c` )
Values: `a` `b` `c`
```