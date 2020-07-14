using System.Collections.Generic;
using NutmegRunner;
using Xunit;

namespace NutmegRunnerTests {

    public class Weave_Codelet_Tests {

        private WovenCodelet Encapsulate( Codelet codelet ) {
            var fc = new FunctionCodelet( 0, 0, codelet );
            return fc.Weave( new ReturnWovenCodelet() );
        }

        [Fact]
        public void Basic_StringCodelet_Weave() {
            //  Arrange
            var arbitraryString = "foo";
            var s = Encapsulate( new StringCodelet( arbitraryString ) );
            var rte = new RuntimeEngine();
            //  Act
            rte.StartFromCodelet(s, false, false);
            //  Assert
            Assert.Equal( 1, rte.ValueStackLength() );
            Assert.Equal( arbitraryString, rte.PeekOrElse() );
        }

    }

}
