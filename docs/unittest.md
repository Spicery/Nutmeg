# Unit Test Framework

## Overview

Nutmeg comes with its own built-in, low-tech unit-testing framework. To write a unit-test, simply add the `@unittest` annotation ahead of a procedure. For example:
```
@unittest
def test_add1():
    assert 3 == 2 + 1
enddef
```
Use the `assert` syntax to check a condition inside a unit-test. If the condition is true then the unit-test passes otherwise it fails and the execution of the unit-test is immediately halted.

To run the unit-tests, run the `nutmeg unittest` command on the bundle-file that includes your unit tests. If the above example is in a file called `mytest.nutmeg` then you would use the following sequence of commands:
```bash
% nutmegc -b mytest.bundle mytest.nutmeg
% nutmeg unittest mytest.bundle
GREEN: 1 passed, 0 failed
```

If your unit tests fail then you get a report on where they failed and some details about how they failed. To show this, let's add a failing unit test to our file:
```
@unittest
def epic_failz():
    assert 'foo' == 'bar'
enddef
```
Now we go through the same commands again but this time we get a failure:
```bash
% nutmegc -b mytest.bundle mytest.nutmeg 
% nutmeg unittest mytest.bundle 
RED: 1 passed, 1 failed
[1] epic_failz, line 8 of mytest.nutmeg: assert 'foo' == 'bar'
  - Left Value: foo
  - Right Value: bar
```

## Technical Summary

The built-in unit-test framework is currently very simple and low-tech. Tests are procedure definitions that are marked with the annotation `@unittest`. The convention is that tests must run without exceptions (and we have no way of testing for exceptions yet, sorry!). 

The `assert` syntax is used to evaluate an expression, which must return `true`, otherwise a special built-in exception is thrown. This special exception contains meta-information that allows the test-runner to show contextual information.

The `assert` syntax also checks whether the expression-under-test is of the form `A == B` or `A != B`. If it is then the values on the left and right hand side are captured in the special exception.

The test runner itself is invoked via `nutmeg unittest`. This runs all the procedures marked as unit-tests and collates the statistics, which are printed to the standard output. (JUnit format is not yet supported, sorry!)