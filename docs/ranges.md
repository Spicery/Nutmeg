# Ranges and Ranged-Lists

## Overview

The range operators in Nutmeg are a shorthand for writing out all the numbers from a starting value to a finishing value. So, instead of writing out all the numbers from 1 to 100 you could just write `1 ... 100`. Or you could pass them to an operator that accepted lots of arguments, like `sum`:
```
sum( 1 ... 100 )
### Returns 5050
```
The other range operator is the "half-open" range, written `..<`. As the '<' suggests, it yields the all values from the start but does not include the finishing value. For example `0 ..< 5` will generate the values `0, 1, 2, 3, 4` but does not include `5`. 

### Combined with List Brackets

Ranges combine especially well with lists. In fact Nutmeg specially recognises the situation when a range is used inside a pair of brackets like this: `[ 10 ... 15 ]`. This is the easiest way to create a special kind of list, called a ranged-list. 

Ranged-lists are the simplest example of a _procedural_ list. Procedural-lists do not expand out all their members, they just calculate their members on demand. If you have `[low ... high]` and ask for the 3rd member, Nutmeg simply adds 3 to low. So the third member of `10 ... 15` would be 13, for example. This means that ranged-lists are very compact and efficient and so are all procedural lists in general.

### Combined with Other Brackets

You can also use ranges with Nutmeg's other brackets, such as set brackets `{? 0 ..< n ?}`, series brackets `[% 0 ..< n %]` and even stream brackets `[: 0 ..< n :]`. All of these will generate procedural collection-like-objects, so you can use large ranges without worrying about using lots of store. 

### Half-open or closed ranges - which is best?

Is it better to use closed-intervals with `...` or half-open intervals like `..<`? The computer scientist Dijkstra [famously argued](https://www.cs.utexas.edu/users/EWD/transcriptions/EWD08xx/EWD831.html) that half-open intervals are more natural. It has certainly persuaded a lot of language-designers that the first member of a list should be numbered 0 and not 1, somewhat counter-intuitively. Although we find Dijkstra's argument to be [not entirely compelling](where-should-numbering-start), Nutmeg follows the same convention to avoid friction for when our coders switch from one language to another. 

Because of this, you will find yourself using the half-open range `0 ..< n` much more often than `1 ... n`. It just fits better when lists start from 0. So it might feel a bit awkward at first but its worth getting used to.

## Technical Summary

### Ranges

Nutmeg has special syntax for two ranges:

* `A ..< B` generates the integer values from A to B, not including B.
* `A ... B` generates the integer values from A to B, including B.

To generate other, more general ranges use the explicit `Range` functions. 

* `Range{ step = 1, test = nonfix < }( initial, final )` - returns an object that is both a list and a  series whose first member is `initial` and whose (n+1)th member M is the nth member plus `step`. The series continues while the `test` is satisfied; the sign of `step` determines the order of arguments. If `step >= 0` then `test( M, final )` is used, otherwise `test( final, M )`.

### Ranged Lists

The expression `[ A ..< B ]` is recognised as syntactic sugar for `Range( A, B )` and `[ A ... B ]` as syntactic sugar for `Range{ test = nonfix <= }( A, B )`. Other brackets also yield procedural analogs e.g. `{? A ..< B ?}` is synonymous with `RangeSet( A, B )`.
