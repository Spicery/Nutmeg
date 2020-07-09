# nutparse.py

## Introduction

nutparse.py -- A recursive descent parser for nutmeg

## Input

- Satements compliant with the [nutmeg grammar](https://github.com/Spicery/Nutmeg/blob/spike/draft_grammar/docs/Nutmeg-prototype-grammar.pdf)

## Output

- JSON representing the Nutmeg codetree: [/codetree-examples](/codetree-examples)

```javascript
{
    'kind': 'if',
    'conditions': {<parse object>},
    'else': {
        'kind': ...
    }
}
```

## Example test cases

| input             | output                                         | notes           |
| ----------------- | ---------------------------------------------- | --------------- |
| 42                | { "type": "int", "value" : "42", "radix": 10 } | integer literal |
| "meaning of life" | { "type": "str", "value" : "meaning of life" } | string literal  |
