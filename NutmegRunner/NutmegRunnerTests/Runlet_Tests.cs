using System;
using NutmegRunner;
using Xunit;


namespace NutmegRunnerTests {

    public class JumpToJump_Runlet_Tests {

        [Fact]
        public void EliminateJ2J_UpdateLink_Test() {
            //  Arrange
            var j = new JumpRunlet( null );
            var p = new PushQRunlet( "foo", j );
            //  Act
            j.UpdateLink( new ReturnRunlet() );
            //  Assert
            Assert.True( p.Next is ReturnRunlet );
        }

        [Fact]
        public void ForkRunlet_EliminateJ2J_UpdateLink_Test() {
            //  Arrange
            var jt = new JumpRunlet( null );
            var je = new JumpRunlet( null );
            var fr = new ForkRunlet( jt, je );
            //  Act0
            jt.UpdateLink( new ReturnRunlet() );
            //  Assert0
            Assert.True( fr.ThenPart is ReturnRunlet );
            Assert.True( fr.ElsePart is JumpRunlet );
            //  Act1
            je.UpdateLink( new HaltRunlet() );
            //  Assert1
            Assert.True( fr.ThenPart is ReturnRunlet );
            Assert.True( fr.ElsePart is HaltRunlet );
        }

    }

}
