using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NutmegRunner {

    public class UncheckedLayeredStack<T> {

        T[] items = new T[1024];
        int layer = 0;
        int top = 0;
        readonly Stack<int> dump = new Stack<int>();

        private void EnsureRoom( int n ) {
            if (n > 0 && this.items.Length < this.top + n) {
                var new_items = new T[this.items.Length * 2 + n];
                Array.Copy( this.items, 0, new_items, 0, this.items.Length );
                this.items = new_items;
            }
        }

        /// <summary>
        /// This method should be called prior to a garbage collection using the GC notification facilities.
        /// </summary>
        public void Trim() {
            //  TODO: call before GC.
            Array.Fill( this.items, default( T ), this.top, this.items.Length - this.top );
        }

        public void Push( T value ) {
            try {
                this.items[this.top] = value;
                this.top += 1;
            } catch (ArgumentOutOfRangeException) {
                this.EnsureRoom( 1 );
                this.items[this.top++] = value;
            }
        }

        public T Pop() {
            try {
                return this.items[--this.top];
            } catch (IndexOutOfRangeException) {
                throw new NutmegException( "Trying to pop empty stack" );
            }
        }

        public void Drop() {
            this.top -= 1;
        }

        public void Clear() {
            this.top = this.layer;
        }

        public void Reset() {
            Array.Clear( this.items, 0, this.items.Length );
            this.layer = 0;
            this.top = 0;
            this.dump.Clear();
        }

        public bool IsEmpty() {
            return this.top == this.layer;
        }

        public int Size() {
            return this.top - this.layer;
        }

        public void Lock() {
            this.dump.Push( this.layer );
            this.layer = this.top;
        }

        public int RawLock( int nlocals, int nargs, UncheckedLayeredStack<T> src ) {
            this.EnsureRoom( nlocals );
            this.Lock();
            src.RawSend( nargs, this );
            if (nlocals >= nargs) {
                //  If this branch is not taken then an error will be raised when nargs is detected to be wrong.
                Array.Fill( this.items, default( T ), this.top + nargs, nlocals - nargs );
                this.top += nlocals;
            } 
            return nargs;
        }

        public void RawReceive( T[] src_data, int src_top, int src_nargs ) {
            Array.Copy( src_data, src_top - src_nargs, this.items, this.top, src_nargs );
        }

        public void Unlock() {
            try {
                this.layer = this.dump.Pop();
            } catch ( InvalidOperationException e ) {
                throw new NutmegException( "Internal error: trying to unlock with no layers", e );
            }
        }

        public void ClearAndUnlock() {
            this.top = this.layer;
            this.layer = this.dump.Pop();
        }

        public int LockCount() {
            return this.dump.Count;
        }

        public T Peek() {
            return this.items[this.top - 1];
        }

        public T PeekOrElse( T orElse = default( T ) ) {
            if (this.top > this.layer) {
                return this.items[this.top - 1];
            } else {
                return orElse;
            }
        }

        public T PeekItem( int n ) {
            return this.items[this.top - n - 1];
        }

        public T PeekItemOrElse( int n, T orElse = default( T ) ) {
            if (this.top > this.layer + n + 1) {
                return this.items[this.top - n - 1];
            } else {
                return orElse;
            }
        }

        public void Update( int n, Func<T, T> f ) {
            int index = this.top - n - 1;
            this.items[index] = f.Invoke( this.items[index] );
        }

        public T this[int n] {
            get {
                return this.items[this.layer + n];
            }
            set {
                this.items[this.layer + n] = value;
            }
        }

        private IEnumerable<object> AllAsEnumerable( int n ) {
            int t = this.top - n;
            for (int i = 0; i < n; i++) {
                yield return this.items[t + i];
            }
        }

        public ImmutableList<object> PopManyToImmutableList( int n ) {
            var all = ImmutableList<object>.Empty.AddRange( this.AllAsEnumerable( n ) );
            this.top -= n;
            return all;
        }

        public List<object> PopManyToList( int m ) {
            var all = new List<object>();
            int t = this.top - m;
            for (int i = 0; i < m; i++) {
                all.Add( this.items[t + i] );
            }
            this.top = t;
            return all;
        }

        public int CountAndUnlock() {
            try {
                int n = this.top - this.layer;
                this.layer = this.dump.Pop();
                return n;
            } catch (InvalidOperationException) {
                throw new NutmegException( "Trying to unlock when there are no locks" );
            }
        }

        public void ApplyUnaryFunction( Func<T, T> f ) {
            if (this.top > this.layer) {
                this.items[this.top - 1] = f( this.items[this.top - 1] );
            } else {
                throw new NutmegException( "No items on stack" );
            }
        }

        public void RawSend( int nargs, UncheckedLayeredStack<T> destination ) {
            var src_data = this.items;
            var src_top = this.top;
            var src_nargs = this.top - this.layer;
            if (src_nargs >= nargs) {
                destination.RawReceive( src_data, src_top, nargs );
                this.top -= nargs;
            } else {
                throw new NutmegException( "Too few arguments" );
            }
        }

        public void DropMany( int n ) {
            this.top -= n;
        }

    }

}
