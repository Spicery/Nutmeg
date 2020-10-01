using System;
using System.Collections;
using System.Collections.Generic;

namespace NutmegRunner {

    public class HalfOpenRangeListEnumerator : IEnumerator<object> {

        long sofar;
        readonly long lo;
        readonly long hi;

        public HalfOpenRangeListEnumerator( long lo, long hi ) {
            this.lo = lo;
            this.sofar = lo;
            this.hi = hi;
        }

        public object Current { get; private set; }

        object IEnumerator.Current => this.Current;

        public void Dispose() {
        }

        public bool MoveNext() {
            if (this.sofar < this.hi) {
                this.Current = this.sofar++;
                return true;
            } else {
                return false;
            }
        }

        public void Reset() {
            this.sofar = this.lo;
            this.Current = default;
        }

    }


    public class HalfOpenRangeList : IReadOnlyList<object> {

        private readonly long lo;
        private readonly long hi;

        public HalfOpenRangeList( long x, long y ) {
            this.lo = x;
            this.hi = Math.Max( x, y );
        }

        public long Low => this.lo;

        public long High => this.hi;

        public object this[int index] {
            get => 0 <= index && index< this.hi? this.lo + index : throw new NutmegException( "Out of range" );
            set => throw new System.NotImplementedException();
        }

        public int Count => (int)(this.hi - lo);

        public bool IsReadOnly => true;

        //public void Add( long item ) {
        //    throw new System.NotImplementedException();
        //}

        //public void Clear() {
        //    throw new System.NotImplementedException();
        //}

        public bool Contains( long item ) {
            return this.lo <= item && item < this.hi;
        }

        public void CopyTo( object[] array, int arrayIndex ) {
            for (long i = this.lo; i < this.hi; i++) {
                array[arrayIndex + i] = i;
            }
        }

        public IEnumerator<object> GetEnumerator() {
            return new HalfOpenRangeListEnumerator(this.lo, this.hi);
        }


        public int IndexOf( long item ) {
            if (this.lo <= item && item < this.hi ) {
                return (int)( item - this.lo );
            } else {
                return -1;
            }
        }

        //public void Insert( int index, long item ) {
        //    throw new System.NotImplementedException();
        //}

        //public bool Remove( long item ) {
        //    throw new System.NotImplementedException();
        //}

        //public void RemoveAt( int index ) {
        //    throw new System.NotImplementedException();
        //}

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator() {
            return this.GetEnumerator();
        }
    }

}