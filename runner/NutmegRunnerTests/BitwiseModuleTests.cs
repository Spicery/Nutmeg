using System;
using Xunit;
using NutmegRunner.Modules.Bitwise;
using NutmegRunner;
using System.Collections.Generic;

namespace NutmegRunnerTests.Modules.Bitwise {
    public class BitwiseModuleTests {

        [Fact]
        public void Basic_ANDSystemFunction_Test() {
            //  Arrange
            var runtime = new RuntimeEngine( false );
            runtime.LockValueStack();
            runtime.PushValue( 0xF0F0L );
            runtime.PushValue( 0xFF00L );
            var list_sysfn = new ANDSystemFunction( null );
            //  Act
            list_sysfn.ExecuteRunlet( runtime );
            //  Assert
            Assert.Equal( 1, runtime.ValueStackLength() );
            object obj = runtime.PopValue1();
            switch (obj) {
                case long x:
                    Assert.Equal( 0xF000L, x );
                    break;
                default:
                    throw new Exception();
            }
        }

        [Fact]
        public void Basic_ORSystemFunction_Test() {
            //  Arrange
            var runtime = new RuntimeEngine( false );
            runtime.LockValueStack();
            runtime.PushValue( 0xF0F0L );
            runtime.PushValue( 0xFF00L );
            var list_sysfn = new ORSystemFunction( null );
            //  Act
            list_sysfn.ExecuteRunlet( runtime );
            //  Assert
            Assert.Equal( 1, runtime.ValueStackLength() );
            object obj = runtime.PopValue1();
            switch (obj) {
                case long x:
                    Assert.Equal( 0xFFF0L, x );
                    break;
                default:
                    throw new Exception();
            }
        }


        [Fact]
        public void Basic_XORSystemFunction_Test() {
            //  Arrange
            var runtime = new RuntimeEngine( false );
            runtime.LockValueStack();
            runtime.PushValue( 0xF0F0L );
            runtime.PushValue( 0xFF00L );
            var list_sysfn = new XORSystemFunction( null );
            //  Act
            list_sysfn.ExecuteRunlet( runtime );
            //  Assert
            Assert.Equal( 1, runtime.ValueStackLength() );
            object obj = runtime.PopValue1();
            switch (obj) {
                case long x:
                    Assert.Equal( 0x0FF0L, x );
                    break;
                default:
                    throw new Exception();
            }
        }


        [Fact]
        public void Basic_NOTSystemFunction_Test() {
            //  Arrange
            var runtime = new RuntimeEngine( false );
            runtime.LockValueStack();
            runtime.PushValue( 0xF0F0L );
            var list_sysfn = new NOTSystemFunction( null );
            //  Act
            list_sysfn.ExecuteRunlet( runtime );
            //  Assert
            Assert.Equal( 1, runtime.ValueStackLength() );
            object obj = runtime.PopValue1();
            switch (obj) {
                case long x:
                    Assert.Equal( 0x0F0FL, x & 0xFFFFL );
                    break;
                default:
                    throw new Exception();
            }
        }

        [Fact]
        public void Basic_LSHIFTSystemFunction_Test() {
            //  Arrange
            var runtime = new RuntimeEngine( false );
            runtime.LockValueStack();
            runtime.PushValue( 0xF0F0L );
            runtime.PushValue( 0x4L );
            var list_sysfn = new LeftShiftSystemFunction( null );
            //  Act
            list_sysfn.ExecuteRunlet( runtime );
            //  Assert
            Assert.Equal( 1, runtime.ValueStackLength() );
            object obj = runtime.PopValue1();
            switch (obj) {
                case long x:
                    Assert.Equal( 0xF0F00L, x );
                    break;
                default:
                    throw new Exception();
            }
        }

        [Fact]
        public void Basic_RSHIFTSystemFunction_Test() {
            //  Arrange
            var runtime = new RuntimeEngine( false );
            runtime.LockValueStack();
            runtime.PushValue( 0xF0F0L );
            runtime.PushValue( 0x4L );
            var list_sysfn = new RightShiftSystemFunction( null );
            //  Act
            list_sysfn.ExecuteRunlet( runtime );
            //  Assert
            Assert.Equal( 1, runtime.ValueStackLength() );
            object obj = runtime.PopValue1();
            switch (obj) {
                case long x:
                    Assert.Equal( 0xF0FL, x );
                    break;
                default:
                    throw new Exception();
            }
        }



    }
}
