
try f(x) catch Foo: null end

switch altcall( f )( x )
else:
    null
endswitch

{
    "kind": "try",
    "subject": { 
        "kind": "trysyscall", 
        "name": "f", 
        "arguments": { "kind": "id", "name": "x" } 
    },
    "cases": [
        [ "Foo", { "kind": "null" } ]
    ],
    "else": { "kind": "seq", "body": [] }
}

How should trysyscall work? It is an alternative entry point for a syscall to the normal one.

i.e. instead of ExecuteRunlet it does TryExecuteRunlet
