using System.Collections.Generic;
using NutmegRunner;
using NutmegRunner.Modules.Ranges;
using NutmegRunner.Modules.Strings;
using Xunit;

namespace NutmegRunnerTests {

    public class HalfOpenRangeSystemFunction_Tests
    {

        [Fact]
        public void Empty_ListSystemFunction_Test() {
            //  Arrange
            var runtime = new RuntimeEngine( false );
            runtime.LockValueStack();
            var list_sysfn = new ListSystemFunction( null );
            //  Act
            list_sysfn.ExecuteRunlet( runtime );
            //  Assert
            Assert.Equal( 1, runtime.ValueStackLength() );
            object list_maybe = runtime.PopValue1();
            var list = list_maybe as IList<object>;
            Assert.NotNull( list );
            Assert.Equal( 0, list.Count );
        }

        [Fact]
        public void Simple_ListSystemFunction_Test() {
            //  Arrange
            var runtime = new RuntimeEngine( false );
            runtime.LockValueStack();
            runtime.PushValue( 0L );
            runtime.PushValue( 0L );
            var list_sysfn = new ListSystemFunction( null );
            //  Act
            list_sysfn.ExecuteRunlet( runtime );
            //  Assert
            Assert.Equal( 1, runtime.ValueStackLength() );
            object list_maybe = runtime.PopValue1();
            var list = list_maybe as IList<object>;
            Assert.NotNull( list );
            Assert.Equal( 2, list.Count );
        }

        [Fact]
        public void Empty_HalfOpenRangeSystemFunction_Test()
        {
            //  Arrange
            var runtime = new RuntimeEngine(false);
            runtime.LockValueStack();
            runtime.PushValue(0L);
            runtime.PushValue(0L);
            var empty = new HalfOpenRangeSystemFunction(null);
            //  Act
            empty.ExecuteRunlet(runtime);
            //  Assert
            Assert.Equal(0, runtime.ValueStackLength());
        }

        [Fact]
        public void Standard_HalfOpenRangeSystemFunction_Test()
        {
            //  Arrange
            var runtime = new RuntimeEngine(false);
            runtime.LockValueStack();
            runtime.PushValue(10L);
            runtime.PushValue(15L);
            var empty = new HalfOpenRangeSystemFunction(null);
            //  Act
            empty.ExecuteRunlet(runtime);
            //  Assert
            Assert.Equal(5, runtime.ValueStackLength());
            Assert.Equal(14L, runtime.PopValue());
            Assert.Equal(13L, runtime.PopValue());
            Assert.Equal(12L, runtime.PopValue());
            Assert.Equal(11L, runtime.PopValue());
            Assert.Equal(10L, runtime.PopValue());
        }

    }

    public class ClosedRangeSystemFunction_Tests
    {

        [Fact]
        public void Empty_ClosedRangeSystemFunction_Test()
        {
            //  Arrange
            var runtime = new RuntimeEngine(false);
            runtime.LockValueStack();
            runtime.PushValue(0L);
            runtime.PushValue(-7L);
            var empty = new ClosedRangeSystemFunction(null);
            //  Act
            empty.ExecuteRunlet(runtime);
            //  Assert
            Assert.Equal(0, runtime.ValueStackLength());
        }

        [Fact]
        public void Standard_ClosedRangeSystemFunction_Test()
        {
            //  Arrange
            var runtime = new RuntimeEngine(false);
            runtime.LockValueStack();
            runtime.PushValue(10L);
            runtime.PushValue(15L);
            var empty = new ClosedRangeSystemFunction(null);
            //  Act
            empty.ExecuteRunlet(runtime);
            //  Assert
            Assert.Equal(6, runtime.ValueStackLength());
            Assert.Equal(15L, runtime.PopValue());
            Assert.Equal(14L, runtime.PopValue());
            Assert.Equal(13L, runtime.PopValue());
            Assert.Equal(12L, runtime.PopValue());
            Assert.Equal(11L, runtime.PopValue());
            Assert.Equal(10L, runtime.PopValue());
        }

    }


    public class HalfOpenRangeList_SystemFunction_Tests
    {

        [Fact]
        public void Empty_HalfOpenRangeListSystemFunction_Test()
        {
            //  Arrange
            var runtime = new RuntimeEngine(false);
            runtime.LockValueStack();
            runtime.PushValue(0L);
            runtime.PushValue(0L);
            var empty = new HalfOpenRangeListSystemFunction(null);
            //  Act
            empty.ExecuteRunlet(runtime);
            var result = (IReadOnlyList<object>)runtime.PopValue1();
            //  Assert
            Assert.Equal(0, result.Count);
        }

        [Fact]
        public void Standard_HalfOpenRangeListSystemFunction_Test()
        {
            //  Arrange
            var runtime = new RuntimeEngine(false);
            runtime.LockValueStack();
            runtime.PushValue(10L);
            runtime.PushValue(15L);
            var empty = new HalfOpenRangeListSystemFunction(null);
            //  Act
            empty.ExecuteRunlet(runtime);
            var result = (IReadOnlyList<object>)runtime.PopValue1();
            //  Assert
            Assert.Equal(5, result.Count);
            Assert.Equal(10L, result[0]);
            Assert.Equal(14L, result[4]);
        }

        [Fact]
        public void StandardEnumerator_HalfOpenRangeListSystemFunction_Test()
        {
            //  Arrange
            var runtime = new RuntimeEngine(false);
            runtime.LockValueStack();
            runtime.PushValue(10L);
            runtime.PushValue(15L);
            var empty = new HalfOpenRangeListSystemFunction(null);
            //  Act
            empty.ExecuteRunlet(runtime);
            var result = (IReadOnlyList<object>)runtime.PopValue1();
            //  Assert
            var n = 10L;
            foreach (var i in result)
            {
                Assert.Equal(n++, i);
            }
        }

    }


    public class ClosedRangeList_SystemFunction_Tests
    {

        [Fact]
        public void Empty_ClosedRangeListSystemFunction_Test()
        {
            //  Arrange
            var runtime = new RuntimeEngine(false);
            runtime.LockValueStack();
            runtime.PushValue(0L);
            runtime.PushValue(-7L);
            var empty = new ClosedRangeListSystemFunction(null);
            //  Act
            empty.ExecuteRunlet(runtime);
            var result = (IReadOnlyList<object>)runtime.PopValue1();
            //  Assert
            Assert.Equal(0, result.Count);
        }

        [Fact]
        public void Standard_ClosedRangeListSystemFunction_Test()
        {
            //  Arrange
            var runtime = new RuntimeEngine(false);
            runtime.LockValueStack();
            runtime.PushValue(10L);
            runtime.PushValue(15L);
            var empty = new ClosedRangeListSystemFunction(null);
            //  Act
            empty.ExecuteRunlet(runtime);
            var result = (IReadOnlyList<object>)runtime.PopValue1();
            //  Assert
            Assert.Equal(6, result.Count);
            Assert.Equal(10L, result[0]);
            Assert.Equal(15L, result[5]);
        }

        [Fact]
        public void StandardEnumerator_ClosedRangeListSystemFunction_Test()
        {
            //  Arrange
            var runtime = new RuntimeEngine(false);
            runtime.LockValueStack();
            runtime.PushValue(10L);
            runtime.PushValue(15L);
            var empty = new ClosedRangeListSystemFunction(null);
            //  Act
            empty.ExecuteRunlet(runtime);
            var result = (IReadOnlyList<object>)runtime.PopValue1();
            //  Assert
            var n = 10L;
            foreach (var i in result)
            {
                Assert.Equal(n++, i);
            }
            Assert.Equal(16L, n);
        }


        [Fact]
        public void String_Get_Test() {
            //  Arrange
            var runtime = new RuntimeEngine( false );
            runtime.LockValueStack();
            runtime.PushValue( "foo" );
            runtime.PushValue( 0L );
            var empty = new StringGet( null );
            //  Act
            empty.ExecuteRunlet( runtime );
            var c = runtime.PopValue1();
            //  Assert
            Assert.Equal( "foo"[0], c );
        }

    }


}
