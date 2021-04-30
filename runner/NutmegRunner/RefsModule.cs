using System;

namespace NutmegRunner.Modules.Refs {

    public interface INutmegObject {
        public bool IsSealed { get; }
        public void Seal();
    }

    public class AbsSealableNutmegObject : INutmegObject {
        protected bool _isLocked = false;
        public bool IsSealed => _isLocked;
        public void Seal() { _isLocked = true; }
    }

    public abstract class AbsRef : AbsSealableNutmegObject {
        protected object _item;
        public AbsRef( object x ) {
            this._item = x;
        }
        public Object GetItem() => this._item;
    }

    public class ValRef : AbsRef {
        public ValRef( object obj ) : base( obj ) {
            this._isLocked = true;  //  Pre-sealed.
        }
    }

    public class VarRef : AbsRef {
        public VarRef( Object obj ) : base( obj ) { }
        public void SetItem( object x ) {
            if (this.IsSealed) throw new NutmegException( "Trying to update a locked object" ).Culprit( "Item", $"{this}" );
            this._item = x;
        }
    }

    public class NewVarRef : UnarySystemFunction {

        public NewVarRef( Runlet next ) : base( next ) { }

        public override object Apply( object x ) {
            return new VarRef( x );
        }

    }

    public class NewValRef : UnarySystemFunction {

        public NewValRef( Runlet next ) : base( next ) { }

        public override object Apply( object x ) {
            return new ValRef( x );
        }

    }

    public class LockObjectSystemFunction : FixedAritySystemFunction {

        public LockObjectSystemFunction( Runlet next ) : base( next ) { }

        public override int Nargs => 1;

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            INutmegObject nmo = (INutmegObject)runtimeEngine.PeekOrElse();
            nmo?.Seal();
            return this.Next;
        }

    }

    public class IsRefSystemFunction : UnarySystemFunction {

        public IsRefSystemFunction( Runlet next ) : base( next ) { }

        public override object Apply( object x ) {
            return typeof( AbsRef ).IsInstanceOfType( x );
        }

    }

    public class ItemRefSystemFunction : UnarySystemFunction, IFixedAritySystemUpdater {

        public ItemRefSystemFunction( Runlet next ) : base( next ) { }

        public override object Apply( object x ) {
            return ((AbsRef)x).GetItem();
        }

        public override Runlet ExecuteRunlet( RuntimeEngine runtimeEngine ) {
            runtimeEngine.ApplyUnaryFunction( x => ((AbsRef)x).GetItem() );
            return this.Next;
        }

        public override Runlet UpdateRunlet( RuntimeEngine runtimeEngine ) {
            var v = runtimeEngine.PopValue();
            var r = runtimeEngine.PopValue();
            //Console.WriteLine( $"v = {v}, r = {r}" );
            ( (VarRef)r).SetItem( v );
            return this.Next;
        }

        public (int, int) UNargs => (1, 1);

    }

    public class SetItemRefSystemFunction : UnaryToVoidSystemFunction {
        public SetItemRefSystemFunction( Runlet next ) : base( next ) { }
        public override void Apply( object x ) {
            ((VarRef)x).SetItem( x );
        }
    }

    public class RefsModule : SystemFunctionsModule {
        public override void AddAll() {
            Add( "newVarRef", r => new NewVarRef( r ) );
            Add( "newValRef", r => new NewValRef( r ) );
            Add( "lockObject", r => new LockObjectSystemFunction( r ) );
            Add( "isRef", r => new IsRefSystemFunction( r ) );
            Add( "itemRef", r => new ItemRefSystemFunction( r ) );
            Add( "setItemRef", r => new SetItemRefSystemFunction( r ) );
        }
    }
}
