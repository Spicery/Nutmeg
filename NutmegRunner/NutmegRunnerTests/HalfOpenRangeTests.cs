using NutmegRunner;
using Xunit;

namespace NutmegRunnerTests {

    public class HalfOpenRange_Tests {

        [Fact]
        public void Empty_Index() {
            //  Arrange
            var r = new HalfOpenRangeList( 0L, -7L );
            //  Act
            //  Assert
            Assert.Empty( r );
            Assert.True( 0 == r.Count ); // Have to use True and not Equal as an bad warning gets in the way.
            Assert.ThrowsAny<NutmegException>( () => r[0] );
            Assert.Equal( -1L, r.IndexOf( 6L ) );
        }

        [Fact]
        public void Standard_Index() {
            //  Arrange
            var r = new HalfOpenRangeList( 0L, 5L );
            //  Act
            var n = r[2];
            //  Assert
            Assert.Equal( 5, r.Count );
            Assert.Equal( 2L, n );
            for ( long i = 0L; i < 5L; i++ ) {
                Assert.True( r.Contains( i ) );
            }
            Assert.False( r.Contains( -1L ) );
            Assert.Equal( -1L, r.IndexOf( 6L ) );
            Assert.Equal( 2, r.IndexOf( 2L ) );
        }

        [Fact]
        public void Empty_Enumerate() {
            //  Arrange
            var r = new HalfOpenRangeList( 0L, -7L );
            //  Act
            var n = 0;
            foreach (var i in r) {
                n += 1;
            }
            //  Assert
            Assert.Equal( 0, n );
        }

        [Fact]
        public void Standard_Enumerate() {
            //  Arrange
            var r = new HalfOpenRangeList( 10L, 15L );
            //  Act
            var sum = 0L;
            var n = 0;
            foreach (var i in r) {
                n += 1;
                sum += (long)i;
            }
            //  Assert
            Assert.Equal( 5, n );
            Assert.Equal( 60, sum );
        }

    }

}
