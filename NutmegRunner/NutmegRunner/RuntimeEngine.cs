using System;
using System.Collections.Generic;

namespace NutmegRunner {

    public class RuntimeEngine {

        Stack<object> _valueStack = new Stack<object>();

        public RuntimeEngine() {
        }

        public void Push( string value ) {
            this._valueStack.Push( value );
        }

        public object Pop() {
            return this._valueStack.Pop();
        }

    }

}
