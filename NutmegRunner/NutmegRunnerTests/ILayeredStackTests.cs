using System;
using System.Collections.Generic;
using NutmegRunner;
using Xunit;

namespace NutmegRunnerTests {

    public class CheckedLayeredStack_Tests {

        [Fact]
        public void StartsEmpty_IsEmpty_Test() {
            //  Arrange
            var stack = new CheckedLayeredStack<string>();
            //  Act
            var isEmpty = stack.IsEmpty();
            //  Assert
            Assert.True( isEmpty );
        }

        [Fact]
        public void CannotPopEmpty_Pop_Test() {
            //  Arrange
            var stack = new CheckedLayeredStack<string>();
            //  Assert
            Assert.Throws<NutmegException>( () => stack.Pop() );
        }

        [Fact]
        public void Simple_PeekAndPeekItem_Test() {
            //  Arrange
            var stack = new CheckedLayeredStack<string>();
            //  Act
            stack.Push( "a" );
            stack.Push( "b" );
            var b = stack.Peek();
            var a = stack.PeekItem( 1 );
            //  Assert
            Assert.Equal( "b", b );
            Assert.Equal( "a", a );
        }

        [Fact]
        public void SimplePop_Pop_Test() {
            //  Arrange
            var stack = new CheckedLayeredStack<string>();
            stack.Push( "a" );
            stack.Push( "b" );
            //  Act
            var b = stack.Pop();
            var a = stack.Pop();
            //  Assert
            Assert.True( stack.IsEmpty() );
            Assert.Equal( 0, stack.Size() );
            Assert.Equal( "b", b );
            Assert.Equal( "a", a );
        }

        [Fact]
        public void CannotPeekEmpty_Peek_Test() {
            //  Arrange
            var stack = new CheckedLayeredStack<string>();
            //  Assert
            Assert.Throws<NutmegException>( () => stack.Peek() );
        }

        [Fact]
        public void CanPeekOrElseEmpty_PeekOrElse_Test() {
            //  Arrange
            var stack = new CheckedLayeredStack<string>();
            var arbitrary = "foobar";
            //  Act
            var x = stack.PeekOrElse( orElse: arbitrary );
            var y = stack.PeekItemOrElse( 0, orElse: arbitrary );
            //  Assert
            Assert.Equal( arbitrary, x );
            Assert.Equal( arbitrary, y );
        }


        [Fact]
        public void StartsEmptyLocked_IsEmpty_Test() {
            //  Arrange
            var stack = new CheckedLayeredStack<string>();
            stack.Push( "p" );
            stack.Push( "q" );
            stack.Lock();
            //  Act
            var isEmpty = stack.IsEmpty();
            var k = stack.LockCount();
            //  Assert
            Assert.True( isEmpty );
            Assert.Equal( 1, k );
        }

        [Fact]
        public void CannotPopEmptyLocked_Pop_Test() {
            //  Arrange
            var stack = new CheckedLayeredStack<string>();
            stack.Push( "p" );
            stack.Push( "q" );
            stack.Lock();            //  Assert
            Assert.Throws<NutmegException>( () => stack.Pop() );
        }

        [Fact]
        public void SimpleLocked_PeekAndPeekItem_Test() {
            //  Arrange
            var stack = new CheckedLayeredStack<string>();
            stack.Push( "p" );
            stack.Push( "q" );
            stack.Lock();            //  Act
            stack.Push( "a" );
            stack.Push( "b" );
            var b = stack.Peek();
            var a = stack.PeekItem( 1 );
            //  Assert
            Assert.Equal( "b", b );
            Assert.Equal( "a", a );
        }

        [Fact]
        public void SimplePopLocked_Pop_Test() {
            //  Arrange
            var stack = new CheckedLayeredStack<string>();
            stack.Push( "p" );
            stack.Push( "q" );
            stack.Lock();
            stack.Push( "a" );
            stack.Push( "b" );
            //  Act
            var b = stack.Pop();
            var a = stack.Pop();
            //  Assert
            Assert.True( stack.IsEmpty() );
            Assert.Equal( 0, stack.Size() );
            Assert.Equal( "b", b );
            Assert.Equal( "a", a );
        }

        [Fact]
        public void CannotPeekEmptyLocked_Peek_Test() {
            //  Arrange
            var stack = new CheckedLayeredStack<string>();
            stack.Push( "p" );
            stack.Push( "q" );
            stack.Lock();
            //  Assert
            Assert.Throws<NutmegException>( () => stack.Peek() );
        }

        [Fact]
        public void CanPeekOrElseEmptyLocked_PeekOrElse_Test() {
            //  Arrange
            var stack = new CheckedLayeredStack<string>();
            stack.Push( "p" );
            stack.Push( "q" );
            stack.Lock();
            var arbitrary = "foobar";
            //  Act
            var x = stack.PeekOrElse( orElse: arbitrary );
            var y = stack.PeekItemOrElse( 0, orElse: arbitrary );
            //  Assert
            Assert.Equal( arbitrary, x );
            Assert.Equal( arbitrary, y );
        }


        [Fact]
        public void CanUnlock_Unlock_Test() {
            //  Arrange
            var stack = new CheckedLayeredStack<string>();
            stack.Push( "p" );
            stack.Push( "q" );
            stack.Lock();
            //  Act
            stack.Unlock();
            //  Assert
            Assert.Equal( 0, stack.LockCount() );
            Assert.Equal( 2, stack.Size() );
        }

        [Fact]
        public void CanLockAndUnlockEmptyLayer_Lock_Test() {
            //  Arrange
            var stack = new CheckedLayeredStack<string>();
            //  Act
            stack.Lock();
            //  Assert
            Assert.True( stack.IsEmpty() );
            Assert.Equal( 1, stack.LockCount() );
            //  Act
            stack.Unlock();
            //  Assert
            Assert.True( stack.IsEmpty() );
            Assert.Equal( 0, stack.LockCount() );
            //  Assert
            Assert.Throws<NutmegException>( () => stack.Unlock() );
        }

    }


    public class UncheckedLayeredStack_Tests {

        [Fact]
        public void Index2Stack_Indexer_Test() {
            //  Arrange
            var stack = new UncheckedLayeredStack<string>();
            stack.Push( "a" );
            stack.Push( "b" );
            //  Act
            var a = stack[0];
            var b = stack[1];
            //  Assert
            Assert.Equal( "a", a );
            Assert.Equal( "b", b );
        }

        [Fact]
        public void StartsEmpty_IsEmpty_Test() {
            //  Arrange
            var stack = new UncheckedLayeredStack<string>();
            //  Act
            var isEmpty = stack.IsEmpty();
            //  Assert
            Assert.True( isEmpty );
        }

        [Fact]
        public void CannotPopEmpty_Pop_Test() {
            //  Arrange
            var stack = new UncheckedLayeredStack<string>();
            //  Assert
            Assert.Throws<NutmegException>( () => stack.Pop() );
        }

        [Fact]
        public void Simple_PeekAndPeekItem_Test() {
            //  Arrange
            var stack = new UncheckedLayeredStack<string>();
            //  Act
            stack.Push( "a" );
            stack.Push( "b" );
            var b = stack.Peek();
            var a = stack.PeekItem( 1 );
            //  Assert
            Assert.Equal( "b", b );
            Assert.Equal( "a", a );
        }

        [Fact]
        public void SimplePop_Pop_Test() {
            //  Arrange
            var stack = new UncheckedLayeredStack<string>();
            stack.Push( "a" );
            stack.Push( "b" );
            //  Act
            var b = stack.Pop();
            var a = stack.Pop();
            //  Assert
            Assert.True( stack.IsEmpty() );
            Assert.Equal( 0, stack.Size() );
            Assert.Equal( "b", b );
            Assert.Equal( "a", a );
        }

        [Fact]
        public void CannotPeekEmpty_Peek_Test() {
            //  Arrange
            var stack = new UncheckedLayeredStack<string>();
            //  Assert
            Assert.Throws<NutmegException>( () => stack.Peek() );
        }

        [Fact]
        public void CanPeekOrElseEmpty_PeekOrElse_Test() {
            //  Arrange
            var stack = new UncheckedLayeredStack<string>();
            var arbitrary = "foobar";
            //  Act
            var x = stack.PeekOrElse( orElse: arbitrary );
            var y = stack.PeekItemOrElse( 0, orElse: arbitrary );
            //  Assert
            Assert.Equal( arbitrary, x );
            Assert.Equal( arbitrary, y );
        }


        [Fact]
        public void StartsEmptyLocked_IsEmpty_Test() {
            //  Arrange
            var stack = new UncheckedLayeredStack<string>();
            stack.Push( "p" );
            stack.Push( "q" );
            stack.Lock();
            //  Act
            var isEmpty = stack.IsEmpty();
            var k = stack.LockCount();
            //  Assert
            Assert.True( isEmpty );
            Assert.Equal( 1, k );
        }

        [Fact]
        public void CannotPopEmptyLocked_Pop_Test() {
            //  Arrange
            var stack = new UncheckedLayeredStack<string>();
            stack.Push( "p" );
            stack.Push( "q" );
            stack.Lock();
            //  Assert
            stack.Pop();    //  Does not throw an exception. Unchecked.
        }

        [Fact]
        public void SimpleLocked_PeekAndPeekItem_Test() {
            //  Arrange
            var stack = new UncheckedLayeredStack<string>();
            stack.Push( "p" );
            stack.Push( "q" );
            stack.Lock();            //  Act
            stack.Push( "a" );
            stack.Push( "b" );
            var b = stack.Peek();
            var a = stack.PeekItem( 1 );
            //  Assert
            Assert.Equal( "b", b );
            Assert.Equal( "a", a );
        }

        [Fact]
        public void SimplePopLocked_Pop_Test() {
            //  Arrange
            var stack = new UncheckedLayeredStack<string>();
            stack.Push( "p" );
            stack.Push( "q" );
            stack.Lock();
            stack.Push( "a" );
            stack.Push( "b" );
            //  Act
            var b = stack.Pop();
            var a = stack.Pop();
            //  Assert
            Assert.True( stack.IsEmpty() );
            Assert.Equal( 0, stack.Size() );
            Assert.Equal( "b", b );
            Assert.Equal( "a", a );
        }

        [Fact]
        public void CannotPeekEmptyLocked_Peek_Test() {
            //  Arrange
            var stack = new UncheckedLayeredStack<string>();
            stack.Push( "p" );
            stack.Push( "q" );
            stack.Lock();
            //  Assert
            stack.Peek();   // No exceptio thrown - unchecked.
        }

        [Fact]
        public void CanPeekOrElseEmptyLocked_PeekOrElse_Test() {
            //  Arrange
            var stack = new UncheckedLayeredStack<string>();
            stack.Push( "p" );
            stack.Push( "q" );
            stack.Lock();
            var arbitrary = "foobar";
            //  Act
            var x = stack.PeekOrElse( orElse: arbitrary );
            var y = stack.PeekItemOrElse( 0, orElse: arbitrary );
            //  Assert
            Assert.Equal( arbitrary, x );
            Assert.Equal( arbitrary, y );
        }


        [Fact]
        public void CanUnlock_Unlock_Test() {
            //  Arrange
            var stack = new UncheckedLayeredStack<string>();
            stack.Push( "p" );
            stack.Push( "q" );
            stack.Lock();
            //  Act
            stack.Unlock();
            //  Assert
            Assert.Equal( 0, stack.LockCount() );
            Assert.Equal( 2, stack.Size() );
        }

        [Fact]
        public void UncheckedLayeredStack() {
            //  Arrange
            var stack = new CheckedLayeredStack<string>();
            //  Act
            stack.Lock();
            //  Assert
            Assert.True( stack.IsEmpty() );
            Assert.Equal( 1, stack.LockCount() );
            //  Act
            stack.Unlock();
            //  Assert
            Assert.True( stack.IsEmpty() );
            Assert.Equal( 0, stack.LockCount() );
            //  Assert
            Assert.Throws<NutmegException>( () => stack.Unlock() );
        }

    }


    public class LayeredStackTransfer_Tests {

        [Fact]
        public void Simple_RawLock_Test() {
            //  Arrange
            var vstack = new CheckedLayeredStack<string>();
            vstack.Lock();
            vstack.Push( "a" );
            vstack.Push( "b" );
            var cstack = new UncheckedLayeredStack<string>();
            //  Act
            cstack.RawLock( 4, vstack );
            //  Assert
            Assert.Equal( 1, vstack.LockCount() );
            Assert.Equal( 0, vstack.Size() );
            Assert.Equal( 1, cstack.LockCount() );
            Assert.Equal( 4, cstack.Size() );
            Assert.Equal( "a", cstack[0] );
            Assert.Equal( "b", cstack[1] );
            Assert.Null( cstack[2] );
            Assert.Null( cstack[3] );
        }

    }

}