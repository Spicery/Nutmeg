using System.Collections.Generic;
using NutmegRunner;
using Xunit;

namespace NutmegRunnerTests {

    public class Weave_Codelet_Tests {

        private Runlet Encapsulate( Codelet codelet ) {
            var fc = new LambdaCodelet( 0, 0, codelet );
            return fc.Weave( new ReturnRunlet(), null );
        }

        [Fact]
        public void Basic_StringCodelet_Weave() {
            //  Arrange
            var arbitraryString = "foo";
            var s = Encapsulate( new StringCodelet( arbitraryString ) );
            var rte = new RuntimeEngine(false);
            //  Act
            rte.StartFromCodelet(s, new List<string>(), useEvaluate: false, usePrint: false);
            //  Assert
            Assert.Equal( 1, rte.ValueStackLength() );
            Assert.Equal( arbitraryString, rte.PeekOrElse() );
        }

    }

}
