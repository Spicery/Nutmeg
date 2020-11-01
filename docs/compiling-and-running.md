# Compiling and Running Nutmeg

## Overview

At this early stage of development, Nutmeg comes as an old-school compiler and interpreter (aka "runner"). The output of the compiler is a single self-contained 'bundle-file', which is actually a [SQLITE](https://sqlite.org) database. The compiler does not generate native code but an intermediate format that can be interpreted efficiently.

The interpreter loads the compiled program from the bundle-file and executes it immediately. Any options for the interpreter itself must come before the bundle-file and any arguments for the Nutmeg program must come after.

## Compiler: nutmegc

We compile code using the `nutmegc` command, which is just a special case of `nutmeg compile`. The basic usage is:
```
usage: nutmegc [-h] [-k] -b BUNDLE FILES

positional arguments:
  FILES                 Nutmeg source files to compile

optional arguments:
  -h, --help            Show this help message and exit
  -b, --bundle BUNDLE   The bundle-file to output (or modify)
  -k, --keep            If bundle file already exists, do not clear it
```

The typical compile command looks like this and will create a bundle file (`-b`) called `myprog.bundle`.
```bash
% nutmegc -b myprog.bundle file1.nutmeg file2.nutmeg ...
```
The bundle-file that is created contains all the resources that are needed - and there are no additional files created that have to be separately managed.


## Runner/Interpreter

To run the newly created bundle-file, you simply invoke the `nutmeg` command with the bundle-file as the first argument (not counting options).
```
% nutmeg myapp.bundle ARGUMENT1 ARGUMENT2 ARGUMENT3 ...
```
The additional arguments that follow the bundle file are passed to the Nutmeg program as an immutable list of strings, as you might expect. 

You mark the procedure that will receive the arguments with the `@command` annotation. If this procedure takes no arguments then the argument list must be empty or an error will be thrown.
