using System;
using System.Collections;
using System.Collections.Generic;

namespace NutmegRunner {

    public abstract class FromToByEnumerator : IEnumerator<object> {

        protected long _sofar;
        readonly long _start;
        protected readonly long _end;
        readonly long _step;

        public FromToByEnumerator( long lo, long hi, long step ) {
            this._start = lo;
            this._sofar = lo;
            this._end = hi;
            if (step == 0) throw new NutmegException( "Zero step for half-open range" ).Culprit( "Step", $"{step}" ).Hint( "Step must be non-zero" );
            this._step = step;
        }

        public object Current { get; private set; }

        object IEnumerator.Current => this.Current;

        public void Dispose() {
        }

        protected abstract bool HasNext();

        public bool MoveNext() {
            if (this.HasNext()) {
                this.Current = this._sofar;
                this._sofar += this._step;
                return true;
            } else {
                return false;
            }
        }

        public void Reset() {
            this._sofar = this._start;
            this.Current = default;
        }
    }

    public class UpToByEnumerator : FromToByEnumerator {

        public UpToByEnumerator( long lo, long hi, long step ) : base( lo, hi, step ){
            if ( step <= 0  ) throw new NutmegException( "Invalid step for half-open range" ).Culprit( "Step", $"{step}" ).Hint( "Step must be greater than zero" );
        }

        protected override bool HasNext() {
            return this._sofar < this._end;
        }

    }

    public class DownToByEnumerator : FromToByEnumerator {

        public DownToByEnumerator( long lo, long hi, long step ) : base( lo, hi, step ) {
            if (step > 0) throw new NutmegException( "Invalid step for half-open range" ).Culprit( "Step", $"{step}" ).Hint( "Step must be less than zero" );
        }

        protected override bool HasNext() {
            return this._sofar > this._end;
        }
    }

    public abstract class HalfOpenRangeList : IReadOnlyList<object> {

        protected readonly long _start;
        protected readonly long _end;
        protected readonly long _step;
        private readonly long _count;

        public HalfOpenRangeList( long x, long y, long step = 1 ) {
            if (step == 0) throw new NutmegException( "Zero step for half-open range" ).Culprit( "Step", $"{step}" ).Hint( "Step must be non-zero" );
            this._start = x;
            this._end = step > 0 ? Math.Max( x, y ) : Math.Min( x, y );
            this._step = step;
            var direction = step > 0 ? 1 : -1;
            this._count = (this._end - this._start + step - direction) / step;
        }

        public static HalfOpenRangeList New( long x, long y, long step = 1 ) {
            if ( step > 0 ) {
                return new UpToByList( x, y, step );
            } else if ( step < 0 ) {
                return new DownToByList( x, y, step );
            } else {
                throw new NutmegException( "Zero step for half-open range" ).Culprit( "Step", $"{step}" ).Hint( "Step must be non-zero" );
            }
        }

        public long Start => this._start;

        public long End => this._end;

        public long Step => this._step;

        public bool Ascending => this._step > 0;

        public object this[int index] {
            get => 0 <= index && index < this._count ? this._start + index * this._step : throw new NutmegException( "Out of range" );
            set => throw new System.NotImplementedException();
        }

        public int Count => (int)(this._count);

        public bool IsReadOnly => true;

        public abstract bool Contains( long item );

        public void CopyTo( object[] array, int arrayIndex ) {
            for (long i = 0; i < this._count; i++) {
                array[arrayIndex + i] = this._start + i * this._step;
            }
        }

        public abstract IEnumerator<object> GetEnumerator();

        public int IndexOf( long item ) {
            if (this.Contains( item )) {
                return (int)((item - this._start) / this._step);
            } else {
                return -1;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator() {
            return this.GetEnumerator();
        }
    }

    public class UpToByList : HalfOpenRangeList {

        public UpToByList( long x, long y, long step = 1 ) : base( x, y, step ) {
            if (step <= 0) throw new NutmegException( "Invalid step for half-open range" ).Culprit( "Step", $"{step}" ).Hint( "Step must be greater than zero" );
        }

        public override IEnumerator<object> GetEnumerator() {
            return new UpToByEnumerator( this._start, this._end, this._step );
        }

        public override bool Contains( long item ) {
            if (this._step == 1) {
                return this._start <= item && item < this._end;
            } else {
                if (item > this._end) return false;
                long n = item - this._start;
                if (n < 0) return false;
                return (n % this._step) == 0;
            }
        }

    }


    public class DownToByList : HalfOpenRangeList {

        public DownToByList( long x, long y, long step = 1 ) : base( x, y, step ) {
            if (step >= 0) throw new NutmegException( "Invalid step for half-open range" ).Culprit( "Step", $"{step}" ).Hint( "Step must be greater than zero" );
        }

        public override IEnumerator<object> GetEnumerator() {
            return new DownToByEnumerator( this._start, this._end, this._step );
        }

        public override bool Contains( long item ) {
            if (this._step == -1) {
                return this._end < item && item <= this._start;
            } else {
                if (item < this._end) return false;
                long d = item - this._start;
                if (d > 0) return false;
                return (d % this._step) == 0;
            }
        }

    }

    //public class HalfOpenRangeList : IReadOnlyList<object> {

    //    private readonly long _lo;
    //    private readonly long _hi;
    //    private readonly long _step;
    //    private readonly long _count;

    //    public HalfOpenRangeList( long x, long y, long step = 1 ) {
    //        if (step == 0) throw new NutmegException( "Zero step for half-open range" ).Culprit( "Step", $"{step}" ).Hint( "Step must be non-zero" );
    //        this._lo = x;
    //        this._hi = step > 0 ? Math.Max( x, y ) : Math.Min( x, y );
    //        this._step = step;
    //        var direction = step > 0 ? 1 : -1;
    //        //Console.WriteLine( $"{this._hi} - {this._lo} + {step} - {direction}" );
    //        this._count = ( this._hi - this._lo + step - direction ) / step;
    //    }

    //    public long Low => this._lo;

    //    public long High => this._hi;

    //    public long Step => this._step;

    //    public bool Ascending => this._step > 0;

    //    public object this[int index] {
    //        get => 0 <= index && index < this._count ? this._lo + index * this._step: throw new NutmegException( "Out of range" );
    //        set => throw new System.NotImplementedException();
    //    }

    //    public int Count => (int)(this._count);

    //    public bool IsReadOnly => true;

    //    public bool Contains( long item ) {
    //        if (Ascending) {
    //            if (item > this._hi) return false;
    //            long n = item - this._lo;
    //            if (n < 0) return false;
    //            return (n % this._step) == 0;
    //        } else {
    //            if ( item < this._hi ) return false;
    //            long d = item - this._lo;
    //            if (d > 0) return false;
    //            return (d % this._step) == 0;
    //        }
    //    }

    //    public void CopyTo( object[] array, int arrayIndex ) {
    //        for (long i = 0; i < this._count; i++) {
    //            array[arrayIndex + i] = this._lo + i * this._step;
    //        }
    //    }

    //    public IEnumerator<object> GetEnumerator() {
    //        if (this._step > 0) {
    //            return new UpToByEnumerator( this._lo, this._hi, this._step );
    //        } else {
    //            return new DownToByEnumerator( this._lo, this._hi, this._step );
    //        }
    //    }

    //    public int IndexOf( long item ) {
    //        if (this.Contains( item )) {
    //            return (int)((item - this._lo) / this._step);
    //        } else {
    //            return -1;
    //        }
    //    }

    //    IEnumerator IEnumerable.GetEnumerator() {
    //        return this.GetEnumerator();
    //    }

    //    IEnumerator<object> IEnumerable<object>.GetEnumerator() {
    //        return this.GetEnumerator();
    //    }
    //}

}