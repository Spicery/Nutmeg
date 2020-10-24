# Assignments

## Overview 

Nutmeg is a bit unusual insofar that it separates assignments into _four_ different forms, and five if you count bindings (`:=`) too. These forms are simple assignment, field update copy-bind and copy-update. Each category has its own syntax and its own pros and cons.

Why does Nutmeg have more than one assignment operator? It is partly because the Nutmeg language makes very careful distinctions between side-effecting and non-side-effecting code, it needs both destructive and non-destructive assignment. And it also makes an important distinction between updating local `var` variables, whose changes are private to a procedure and are which are guaranteed to be disposed of when a procedure exits, and updating objects whose changes are unlimited. These factors give rise to the different operators and their different names.

### Simple Assignments (aka Variable Assignment)

The simplest form is assignment to a `var` variable. This immediately changes the value the variable is bound to. For example:
```
var x = "My name is Steve."
x <- "No wait! It's Fred!"
```
The primary advantage of simple-assignment is that it is very efficient on conventional hardware. It is also very handy for algorithms that are best thought about in terms of changing a small number (2-10) of state variables. And simple assignment is compatible with a procedure being clean, unlike field updates.

One disadvantage is that, in combination with if-statements and loops, it can be all too easy to write hard-to-follow code. Also, Nutmeg places some minor restrictions on the use of `var` variables:

 1. Only local variables can be `var`. Use `let` or `local` to sidestep this limitation.
 2. A nested procedure cannot refer to a `var` variable that was bound outside the procedure.

### Field Updates

Mutable objects have fields that can be updated. To update a field you need to use the field-update syntax, which is written using the `<--` operator.
```
x := MutableList( 'left', 'middle', 'right' )
x[1] <-- 'centre'

println( x )
### Prints: [ 'left', 'centre', 'right' ]
```

The advantage of field-updates is that they are very quick. They give fast, fine-grained control over change. On the other hand, they can be tricky to use safely. This tends to happen when objects are inadvertantly shared (for efficiency) and an update to an object is visible in unintended places. The consequences can be very bad, as you might expect.

Experience has shown time and again that accidental over-sharing of mutable objects is very hard to avoid. So Nutmeg helps the coder manage this risk in a variety of ways: finesses, capsules, clean procedures and copy-updates. Field updates are more powerful than simple assignments and consequently banned, replaced or tamed in these lower risk contexts.


### Copy-Update

Now we come to two very closely related assignments that leave the original object untouched. It is as if they work on a copy of the original object and modify the copy instead. The first of these is copy-update, which uses the syntax `<==`, which is supposed to remind you of the field-update operator `<--`.

```
var x := MutableList( 'left', 'middle', 'right' )
y := x
x[1] <== 'centre'   ### Copy-update

println( x )
println( y )
### Prints
###     [ 'left', 'centre', 'right' ] and then
###     [ 'left', 'middle', 'right' ]
```

The key line `x[1] <== 'centre'` works as if it copys the value, updates the field in the new copy and then assigns the copy to the original variable using simple assignment - hence the name copying-update. 
```
### x[1] <== 'centre' works a bit like this:
val tmp := copy( x )
tmp[1] <-- 'centre'
x <- tmp
```
However, the `<==` operator does not actually need a mutable object. It even works on recursively immutable values! It does of course need a `var` variable to act as the subject.
```
var x := [ 'left', 'middle', 'right' ]  ### Immutable!
x[1] <== 'centre'

println( x )
### Prints: [ 'left', 'centre', 'right' ]
```

The strength of copy-update is that it is far more compatible with functional programming than field-update because it is inherently non-destructive and safer.

The weakness is that it requires making a new object in store and then copying over all the fields, which is much slower than other forms of assignment and requires more memory too. On the other hand, the optimizer can reliably eliminate copying of intermediate values, which can actually mean that clean procedures using copy-update can be as fast as imperative code.


### Copy-Bind

Copy-bind exploits a simple but slightly inobvious trick. It is possible to declare a new variable with the _same name_ as an existing variable. So you could write x := a * b + c like this:
```
x := a       ### Line 1:
x := x * b   ### Line 2: The right hand 'x' is from line 1
x := x + c   ### Line 3: The right hand 'x' is from line 2
```
We can rename the different 'x's in this example to make what is happening clear:
```
x1 := a       ### Line 1:
x2 := x1 * b  ### Line 2: The right hand 'x' is from line 1
x3 := x2 + c  ### Line 3: The right hand 'x' is from line 2
```
So this trick allows us to write code that is visually the same as assignment but without any actual assignments at all. 

(Incidentally, if you try writing code like this, you'll find that the Nutmeg compiler will sometimes issue a warning. This will happen when the sequence of bindings is interrupted by something else. You'll need to reassure the compiler that you're deliberately using the same variable - it will tell you how in the warning message. Copy-bind will never generate this warning though.)

So onto the copy-bind operation itself. The symbol `:==` is used for the copy-bind operation, which is meant to remind you of the shorter bind operator `:=`. Here is how you use it.
```
const x := [ 'left', 'middle', 'right' ]  ### Immutable!
x[1] :== 'straight on'                    ### Copy-bind to a 'new' x.
println( x )
### Print: [ 'left', 'straight on', 'right' ]
```

Just like copy-update, the advantage of copy-bind is that it is completely non-destructive. It can safely be used inside pure functions as it is wholly compatible with functional programming.

And like copy-update, the weakness is that it is more expensive to use than a field-update. It requires making a new object in store and then copying over all the fields, which takes time and memory too. On the other hand, the optimizer will reliably eliminate copying of intermediate values, where possible, which does mean that functions using copying-assignment can be as fast as their imperative counterparts.

Because copy-bind doesn't really do assignment at all but is just a cute way of re-using the same varible name for different variables, over and over again, it is a bit more limited. You can't use it to accumulate across loops, for example. 

## Technical Summary

Nutmeg has five assignment-like operations:

* Binding (`:=`), which introduces a new local variable and its initial value into the current scope.
* Simple assignment (`<-`), which changes the value of `var` variables.
* Field update (`<--`), which alters the field of an object.
* Copy-update (`<==`), a non-destructive copy-and-update operation that works with `var` variables.
* Copy-bind (`:==`), a non-destructive copy-and-update operation that introduces a new variable with the _same name_ as before!

The binding operator adds a new local variable into the local scope which shadows any previous use of that variable in the same scope. The initial value is evaluated _before_ adding this same variable. (And that is why you can reuse the same name repeatedly.)

Aside: If you want to write mutually recursive definitions you will need to use the `def` keyword. See the page on [def](def.md) to learn about writing mutually recursive bindings. 

The simple assignment operator dynamically alters the value bound to a `var` variable. Only variables declared as `var` can be updated and they have several limitations imposed on them for this privilege. It is used like this:
```
var x = 0
x <- 99
```

Field-update is the basic way to alter the value of a mutable object. It is a highly performant, in-place modification of an object. Tt can be tricky to use safe because people make mistakes and inadvertantly write code that over-shares objects so that updates made in one piece of code can suddenly appear where they should not. 

However, it is a vital tool in the imperative programmer's arsenal, so Nutmeg includes it. It is used like this:
```
var r = Ref( "Warts and all" )
r! <-- "Less warts please"
```

Taming field update is what the next assignment operators are all about. Copy-update is a non-destructive update that alters the value of a `var` variable to be a modified copy of the original. The original can be mutable or immutable and the copy will retain that property. It is used like this:
```
var list := [ 2, 4, 8, 15 ]
list[3] <== 16   ### Fix it up
```
You can tell it is non-destructive because if you have any variables pointing to the old value, they will remain intact.
```
var list := [ 2, 4, 8, 15 ]
old_value := list
list[3] <== 16   ### Fix it up
println( old_value )
### Prints [ 2, 4, 8, 15 ]
```

Copy-bind is very similar to copy-update except instead of using simple-assignment it uses binding-to-the-same-name. It is used like this:
```
const list := [ 2, 4, 8, 15 ]
list[3] :== 16   ### Completely new variable with same name 'list'
```
The new variable is declared with the same modifiers as the previous variable with the same name. So in the above example, both `list` variables are declared as `const`.
