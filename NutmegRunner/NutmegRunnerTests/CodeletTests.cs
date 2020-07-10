using NutmegRunner;
using Xunit;

namespace NutmegRunnerTests {
    public class Evaluate_Codelet_Tests
    {
        [Fact]
        public void Basic_StringCodelet_Evaluate()
        {
            //  Arrange
            var arbitraryString = "foo";
            var s = new StringCodelet( arbitraryString );
            var rte = new RuntimeEngine();
            //  Act
            s.Evaluate( rte );
            //  Assert
            Assert.Equal( "foo", rte.PeekOrElse() );
            Assert.Equal( 1, rte.ValueStackLength() );
        }
    }
}
