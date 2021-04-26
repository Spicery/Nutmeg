using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NutmegRunner {

    public class RunletToGraphVizConverter {

        int count = 0;
        readonly ConditionalWeakTable<Runlet, Object> seen = new ConditionalWeakTable<Runlet, Object>();
        readonly ConditionalWeakTable<Runlet, string> named = new ConditionalWeakTable<Runlet, string>();

        private string Name( Runlet r ) {
            if (this.named.TryGetValue( r, out var name )) {
                return name;
            } else {
                var new_name = $"\"{this.count++}. {r.ShortTitle()}\"";
                this.named.Add( r, new_name );
                return new_name;
            }
        }

        private void Convert( Runlet r ) {
            if ( !this.seen.TryGetValue( r, out _ ) ) {
                this.seen.Add( r, r );
                Console.Write( $"{this.Name( r )} -> {{" );
                var neighbors = r.Neighbors().Where( n => n != null ).ToList();
                foreach ( var n in neighbors ) {
                    Console.Write( $" {this.Name(n)}" );
                }
                Console.WriteLine( $" }}" );
                foreach ( var n in neighbors ) {
                    this.Convert( n );
                }
            }
        }

        public void GraphViz( string idName, Runlet r ) {
            Console.WriteLine( $"digraph {idName} {{ node [shape=box];" );
            Convert( r );
            Console.WriteLine( $"}}" );
        }

    }

    public abstract class Runlet
    {
        const string CommonSuffixToStrip = "Runlet";

        public abstract Runlet ExecuteRunlet(RuntimeEngine runtimeEngine);

        public virtual Runlet AltExecuteRunlet( RuntimeEngine runtimeEngine ) {
            var next = this.ExecuteRunlet( runtimeEngine );
            runtimeEngine.PushValue( null );     //   Normal return.
            return next;
        }

        public virtual Runlet Track(Runlet runlet)
        {
            return this;
        }

        public virtual void UpdateLinkNotification()
        {
            //  Skip
        }

        public abstract IEnumerable<Runlet> Neighbors();

        public virtual string ShortTitle() {
            var name = this.GetType().Name;
            name = name.EndsWith( CommonSuffixToStrip ) ? name.Substring( 0, name.Length - CommonSuffixToStrip.Length ) : name;
            if ( name.EndsWith( "SystemFunction" ) ) {
                name = name.Substring( 0, name.Length - "SystemFunction".Length );
                name = $"Sys{name}";
            }
            return name;
        }

    }

    public abstract class RunletWithNext : Runlet
    {

        protected Runlet _next;

        public Runlet Next => this._next;

        public RunletWithNext(Runlet next)
        {
            while (next is JumpRunlet j && j.Next != null)
            {
                next = j.Next;
            }
            this._next = next?.Track(this);
        }

        public override void UpdateLinkNotification()
        {
            while (this._next is JumpRunlet j && j.Next != null)
            {
                this._next = j.Next;
            }
        }

        public override IEnumerable<Runlet> Neighbors() {
            return new List<Runlet> { this.Next };
        }

    }

    /// <summary>
    /// The JumpRunlet is a kind of no-op that simply passes control to
    /// the next runlet. As a consequence we want to elide it wherever
    /// possible. However, the target of a JumpRunlet might be assigned
    /// _after_ it is created - unlike any other Runlet - and indeed that
    /// is the real purpose of the JumpRunlet. When the target is assigned
    /// the nodes that reference it are notified and perform the elision.
    /// </summary>
    public class JumpRunlet : RunletWithNext
    {

        List<Runlet> _tracked = new List<Runlet>();

        public JumpRunlet(Runlet next) : base(next)
        {
        }

        public override Runlet Track(Runlet runlet)
        {
            this._tracked.Add(runlet);
            return this;
        }

        public void UpdateLink(Runlet runlet)
        {
            this._next = runlet;
            foreach (var r in this._tracked)
            {
                r.UpdateLinkNotification();
            }
        }

        public override Runlet ExecuteRunlet(RuntimeEngine runtimeEngine)
        {
            return this._next;
        }

    }

    public class HaltRunlet : Runlet
    {

        bool _usePrint;

        public HaltRunlet(bool usePrint)
        {
            this._usePrint = usePrint;
        }

        public override Runlet ExecuteRunlet(RuntimeEngine runtimeEngine)
        {
            if (this._usePrint)
            {
                var N = runtimeEngine.ValueStackLength();
                Console.WriteLine(N != 1 ? $"There are {N} items returned" : "There is 1 item returned");
                var N1 = N - 1;
                for (int i = 0; i < N; i++)
                {
                    var count = i + 1;
                    var item = runtimeEngine.PeekItemOrElse(N1 - i);
                    Console.Write($"{count}: ");
                    ShowMeSystemFunction.ShowMe(Console.Out, item);
                    Console.WriteLine();
                }
            }
            throw new NormalExitNutmegException();
        }

        public override IEnumerable<Runlet> Neighbors() {
            return new List<Runlet>();
        }
    }

    public class PushIdentRunlet : RunletWithNext
    {

        Ident _ident;

        public PushIdentRunlet(Ident ident, Runlet next) : base(next)
        {
            this._ident = ident;
        }

        public override Runlet ExecuteRunlet(RuntimeEngine runtimeEngine)
        {
            runtimeEngine.PushValue(this._ident.Value);
            return this._next;
        }

    }

    public class PushSlotRunlet : RunletWithNext {
        int _slot;
        public PushSlotRunlet( int slot, Runlet next ) : base( next ) {
            this._slot = slot;
        }
        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            //runtimeEngine.ShowFrames();
            //Console.WriteLine( $"Slot number {this._slot}" );
            runtimeEngine.PushSlot( this._slot );
            //Console.WriteLine( $"PUSHED <<{runtimeEngine.PeekOrElse()}>>" );
            return this._next;
        }
    }

    public class PopSlotRunlet : RunletWithNext {
        int _slot;
        public PopSlotRunlet( int slot, Runlet next ) : base( next ) {
            this._slot = slot;
        }
        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            runtimeEngine.PopSlot( this._slot );
            return this._next;
        }
    }

    public class CallQRunlet : RunletWithNext
    {

        FunctionRunlet _functionRunlet;

        public CallQRunlet(FunctionRunlet fc, Runlet next) : base(next)
        {
            this._functionRunlet = fc;
        }

        public override Runlet ExecuteRunlet(RuntimeEngine runtimeEngine)
        {
            return this._functionRunlet.Call(runtimeEngine, this._next, alt: false );
        }

    }

    public class ReturnRunlet : Runlet
    {

        public override Runlet ExecuteRunlet(RuntimeEngine runtimeEngine)
        {
            return runtimeEngine.Return();
        }

        public override IEnumerable<Runlet> Neighbors() {
            return new List<Runlet>();
        }
    }

    public class LockRunlet : RunletWithNext
    {

        public LockRunlet(Runlet next) : base(next)
        {
        }

        public override Runlet ExecuteRunlet(RuntimeEngine runtimeEngine)
        {
            runtimeEngine.LockValueStack();
            return _next;
        }
    }

    public class UnlockRunlet : RunletWithNext {

        public UnlockRunlet( Runlet next ) : base( next ) {
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            runtimeEngine.UnlockValueStack();
            return _next;
        }
    }

    public class CountAndUnlockRunlet : RunletWithNext {

        public CountAndUnlockRunlet( Runlet next ) : base( next ) {
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            runtimeEngine.CountAndUnlockValueStack();
            return _next;
        }

    }

    public class CheckedUnlockRunlet : RunletWithNext {

        int _nargs;

        public CheckedUnlockRunlet( int nargs, Runlet next ) : base( next ) {
            this._nargs = nargs;
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            if (runtimeEngine.ValueStackLength() == this._nargs) {
                runtimeEngine.UnlockValueStack();
                return _next;
            } else {
                //  TODO: more detailed message needed.
                throw new NutmegException( "Unexpected number of arguments" );
            }
        }

        public override string ShortTitle() {
            return $"CheckedUnlock {this._nargs}";
        }
    }

    public class PopGlobalRunlet : RunletWithNext
    {
        Ident _ident;

        public PopGlobalRunlet(Ident ident, Runlet next) : base(next)
        {
            this._ident = ident;
        }

        public override Runlet ExecuteRunlet(RuntimeEngine runtimeEngine)
        {
            this._ident.Value = runtimeEngine.PopValue1();
            return _next;
        }
    }

    public class PopValueIntoSlotRunlet : RunletWithNext {

        int _slot;

        public PopValueIntoSlotRunlet( int slot, Runlet next ) : base( next ) {
            this._slot = slot;
        }
        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            runtimeEngine.PopValueIntoSlot( this._slot );
            return _next;
        }

    }

    public class Unlock1Runlet : RunletWithNext {

        public Unlock1Runlet( Runlet next ) : base( next ) {
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            runtimeEngine.Unlock1ValueStack();
            return _next;
        }

    }

    public interface ICallable {
        Runlet Call( RuntimeEngine runtimeEngine, Runlet next, bool alt );
        Runlet Update( RuntimeEngine runtimeEngine, Runlet next, bool alt );
    }

    public class PartialApplication : RunletWithNext, ICallable {

        ICallable _fn;
        IList<object> _args;

        public PartialApplication( ICallable fn, IList<object> args, Runlet next ) : base( next ) {
            _fn = fn;
            _args = args;
        }

        public Runlet Call( RuntimeEngine runtimeEngine, Runlet next, bool alt ) {
            runtimeEngine.PushAll( _args );
            return _fn.Call( runtimeEngine, next, alt );
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            runtimeEngine.PushValue( this );
            return this.Next;
        }

        public Runlet Update( RuntimeEngine runtimeEngine, Runlet next, bool alt ) {
            throw new UnimplementedNutmegException();
        }

    }

    public class FunctionRunlet : RunletWithNext, ICallable
    {

        private int Nargs { get; set; }
        public int Nlocals { get; set; }

        private Runlet _startCodelet = null;

        public FunctionRunlet(int nargs, int nlocals, Runlet startCodelet, Runlet next) : base(next)
        {
            this.Nargs = nargs;
            this.Nlocals = nlocals;
            this._startCodelet = startCodelet;
        }

        public Runlet Call(RuntimeEngine runtimeEngine, Runlet next, bool alt )
        {
            runtimeEngine.PushReturnAddress( next, alt: false );
            var nargs = runtimeEngine.CreateFrameAndCopyValueStack(this.Nlocals);
            if (nargs != this.Nargs)
            {
                throw
                    new NutmegException("Mismatch in the number of arguments to the number of parameters")
                    .Culprit("Expected", $"{this.Nargs}")
                    .Culprit("Found", $"{nargs}");
            }
            else
            {
                return this._startCodelet;
            }
        }

        public override Runlet ExecuteRunlet(RuntimeEngine runtimeEngine)
        {
            runtimeEngine.PushValue(this);
            return this.Next;
        }

        public override IEnumerable<Runlet> Neighbors() {
            return new List<Runlet> { this._startCodelet, this._next };
        }

        public Runlet Update( RuntimeEngine runtimeEngine, Runlet next, bool alt ) {
            throw new UnimplementedNutmegException();
        }

    }

    public class UpdateSRunlet : RunletWithNext {

        public UpdateSRunlet( Runlet next ) : base( next ) { }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            var obj = runtimeEngine.PopValue();
            runtimeEngine.CountAndDoubleUnlockValueStack();
            switch ( obj ) {
                case ICallable f:
                    return f.Update( runtimeEngine, this.Next, alt: false );
                default:
                    throw new NutmegException( $"Cannot update this object: {obj}" );
            }
        }

    }

    public class CallSRunlet : RunletWithNext
    {

        public CallSRunlet(Runlet next) : base(next)
        {
        }

        public override Runlet ExecuteRunlet(RuntimeEngine runtimeEngine)
        {
            var obj = runtimeEngine.PopValue();
            runtimeEngine.CountAndUnlockValueStack();
            switch ( obj ) {
                case ICallable f:
                    return f.Call( runtimeEngine, this.Next, alt: false );
                case IEnumerator<object> e:
                    if ( e.MoveNext() ) {
                        runtimeEngine.PushValue( e.Current );
                        return this.Next;
                    } else {
                        throw new NutmegException( $"Stream exhausted: {e}" );
                    }
                default:
                    throw new NutmegException( $"Cannot call this object: {obj}" );
            }
        }
    }

    public class SlotNextRunlet : Runlet {

        readonly int _slot;
        readonly Runlet _okRunlet;
        readonly Runlet _failRunlet;

        public SlotNextRunlet( int slot, Runlet okRunlet, Runlet failRunlet ) {
            this._slot = slot;
            this._okRunlet = okRunlet;
            this._failRunlet = failRunlet;
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            var obj = runtimeEngine.GetSlot( this._slot );
            switch (obj) {
                case FunctionRunlet f:
                    //  TODO: We need to implement AltCall properly for this.
                    throw new UnimplementedNutmegException();
                case IEnumerator<object> e:
                    if (e.MoveNext()) {
                        runtimeEngine.PushValue( e.Current );
                        return this._okRunlet;
                    } else {
                        return this._failRunlet;
                    }
                default:
                    throw new NutmegException( $"Cannot get next from this object: {obj}" );
            }
        }

        public override IEnumerable<Runlet> Neighbors() {
            return new List<Runlet> { this._okRunlet, this._failRunlet };
        }
    }

    public abstract class ShortCircuitRunlet : Runlet {
        protected Runlet RhsPart { get; set; }
        protected Runlet NextPart { get; set; }

        public ShortCircuitRunlet( Runlet rhsPart, Runlet nextPart ) {
            RhsPart = rhsPart;
            NextPart = nextPart;
        }

        public override void UpdateLinkNotification() {
            while (this.RhsPart is JumpRunlet rhsj && rhsj != null) {
                this.RhsPart = rhsj;
            }
            while (this.NextPart is JumpRunlet nextj && nextj != null) {
                this.NextPart = nextj;
            }
        }

        public override IEnumerable<Runlet> Neighbors() {
            return new List<Runlet> { this.RhsPart, this.NextPart };
        }

    }

    public class AndRunlet : ShortCircuitRunlet {
        
        public AndRunlet( Runlet rhsPart, Runlet nextPart ) : base( rhsPart, nextPart ) {
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            return runtimeEngine.PopBoolIf( true ) ? this.RhsPart : this.NextPart;
        }

    }

    public class OrRunlet : ShortCircuitRunlet {

        public OrRunlet( Runlet rhsPart, Runlet nextPart ) : base( rhsPart, nextPart ) {
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            return runtimeEngine.PopBoolIf( false ) ? this.RhsPart : this.NextPart;
        }

    }

    public class ForkRunlet : Runlet
    {

        public Runlet ThenPart { get; set; }
        public Runlet ElsePart { get; set; }

        public ForkRunlet(Runlet thenPart, Runlet elsePart)
        {
            this.ThenPart = thenPart.Track(this);
            this.ElsePart = elsePart.Track(this);
        }

        public override void UpdateLinkNotification()
        {
            while (this.ThenPart is JumpRunlet thenj && thenj.Next != null)
            {
                this.ThenPart = thenj.Next;
            }
            while (this.ElsePart is JumpRunlet elsej && elsej.Next != null)
            {
                this.ElsePart = elsej.Next;
            }
        }

        public override Runlet ExecuteRunlet(RuntimeEngine runtimeEngine)
        {
            return runtimeEngine.PopBool() ? ThenPart : ElsePart;
        }

        public override IEnumerable<Runlet> Neighbors() {
            return new List<Runlet> { ThenPart, ElsePart };
        }
    }

    public class PushQRunlet : RunletWithNext
    {

        private object _value;

        public PushQRunlet(object value, Runlet next) : base(next)
        {
            this._value = value;
        }

        public override Runlet ExecuteRunlet(RuntimeEngine runtimeEngine)
        {
            runtimeEngine.PushValue(this._value);
            return this._next;
        }

        public override string ShortTitle() {
            return $"PushQ {this._value}";
        }

    }

}
