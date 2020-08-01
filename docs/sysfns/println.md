# println( ARGS... ) -> ()

> `println{OPTIONS}( ITEMS... ) -> ()`
> - sep=_separator_
> - term=_terminator_

# Summary

This convenience function is used for sending content to the standard-output, followed by a newline. Each argument in turn has its print-string representation sent to the standard output and then the _terminator_, which is system dependent, is sent as well. The arguments are separated by _separator_.

## Named-Optional Arguments

* `sep=`_separator_: a string that will be printed between arguments to visually separate them. Defaults to a single space.
* `term=`_terminator_: a string that will be printed after the last argument. Defaults to the newline appropriate for the current system.

## Example

For example:

```
>>> println( for i in [ 1..10 ] do i end ) 
1 2 3 4 5 6 7 8 9 10
```
