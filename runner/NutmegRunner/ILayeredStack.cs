using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NutmegRunner {

    public interface ILayeredStack<T> {
        public void Push( T value );
        public T Pop();
        public bool IsEmpty();
        public int Size();
        public void Clear();

        public T Peek();
        public T PeekOrElse( T orElse = default( T ) );
        public T PeekItem( int n );
        public T PeekItemOrElse( int n, T orElse = default( T ) );
        public void Update( int n, Func<T, T> f );

        public void Lock();
        public void Unlock();
        public int LockCount();

        public List<object> PopAllAndUnlock( int n );

        public T this[int n] { get; set; }
    }

    public class CheckedLayeredStack<T> : ILayeredStack<T> {

        T[] items;
        int layer = 0;
        int top = 0;
        readonly Stack<int> dump = new Stack<int>();

        public CheckedLayeredStack( int initialCapacity = 1024 ) {
             this.items = new T[initialCapacity];
        }

        private void EnsureRoom( int n ) {
            if (n > 0 && this.items.Length < this.top + n) {
                var new_items = new T[this.items.Length * 2 + n];
                Array.Copy( this.items, 0, new_items, 0, this.items.Length );
                this.items = new_items;
            }
        }

        /// <summary>
        /// This method should be called prior to a garbage collection using the GC notification facilities.
        /// NOTE: It must only be called at safe points!
        /// </summary>
        public void Trim() {
            Array.Fill( this.items, default( T ), this.top, this.items.Length - this.top );
        }

        public int RawSend( UncheckedLayeredStack<T> destination ) {
            var src_data = this.items;
            var src_top = this.top;
            var src_nargs = this.top - this.layer;
            destination.RawReceive( src_data, src_top, src_nargs );
            this.top = this.layer;                                      // Clear top layer.
            return src_nargs;
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
            if (this.top > this.layer) {
                return this.items[--this.top];
            } else {
                throw new NutmegException( "Not enough values on the stack" );
            }
        }

        public void Drop() {
            if (top <= 0) {
                throw new NutmegException( "Trying to pop empty stack" );
            }
            top -= 1;
        }

        public void Clear() {
            this.top = this.layer;
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

        public void Unlock() {
            try {
                this.layer = this.dump.Pop();
            } catch (InvalidOperationException) {
                throw new NutmegException( "Trying to unlock when there are no locks" );
            }
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

        public int LockCount() {
            return this.dump.Count;
        }

        public T Peek() {
            if (this.top > this.layer) {
                return this.items[this.top - 1];
            } else {
                throw new NutmegException( "No items on stack" );
            }
        }

        public T PeekOrElse( T orElse = default( T ) ) {
            if (this.top > this.layer) {
                return this.items[this.top - 1];
            } else {
                return orElse;
            }
        }

        public T PeekItem( int n ) {
            if (this.top > this.layer + n) {
                return this.items[this.top - n - 1];
            } else {
                throw new NutmegException( "Not enough items on stack" );
            }
        }

        public T PeekItemOrElse( int n, T orElse = default( T ) ) {
            if (this.top > this.layer + n) {
                return this.items[this.top - n - 1];
            } else {
                return orElse;
            }
        }

        public void Update( int n, Func<T, T> f ) {
            int index = this.top - n - 1;
            if (this.top > this.layer + n) {
                this.items[index] = f.Invoke( this.items[index] );
            } else {
                throw new NutmegException( "Not enough items on stack" );
            }
        }

        public T this[int n] {
            get {
                if (this.layer + n < this.top) {
                    return this.items[this.layer + n];
                } else {
                    throw new NutmegException( "Not enough items on stack" );
                }
            }
            set {
                if (this.layer + n < this.top) {
                    this.items[this.layer + n] = value;
                } else {
                    throw new NutmegException( "Not enough items on stack" );
                }
            }
        }



        public List<object> PopAllAndUnlock( int n ) {
            var all = this.PopAll( n );
            this.Unlock();
            return all;
        }

        private IEnumerable<object> AllAsEnumerable( int n ) {
            for (int i = 0; i < n; i++) {
                yield return this.items[this.layer + i];
            }
        }

        public ImmutableList<object> ImmutablePopAll( int n ) {
            var all = ImmutableList<object>.Empty.AddRange( this.AllAsEnumerable( n ) );
            this.top = this.layer;
            return all;
        }

        public List<object> PopAll( int n ) {
            var all = new List<object>();
            for (int i = 0; i < n; i++) {
                all.Add( this.items[this.layer + i] );
            }
            this.top = this.layer;
            return all;
        }

        public List<object> PopMany( int m ) {
            var all = new List<object>();
            var n = this.Size();
            if (0 <= m && m < n) {
                for (int i = n - m; i < n; i++) {
                    all.Add( this.items[this.layer + i] );
                }
                this.top -= m;
                return all;
            } else {
                throw new NutmegException( "Internal error" );
            }
        }

    }

    public class UncheckedLayeredStack<T> : ILayeredStack<T> {

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

        public int RawLock( int nlocals, CheckedLayeredStack<T> src ) {
            this.EnsureRoom( nlocals );
            this.Lock();
            var nargs = src.RawSend( this );
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
            this.layer = this.dump.Pop();
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

        public List<object> PopAllAndUnlock( int n ) {
            var all = new List<object>();
            for (int i = 0; i < n; i++) {
                all.Add( this.items[this.layer + i] );
            }
            this.top = this.layer;
            this.Unlock();
            return all;
        }

    }

}
