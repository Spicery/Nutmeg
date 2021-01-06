using NutmegRunner;
using Xunit;

namespace NutmegRunnerTests {

    public class HalfOpenRange_Tests {

        [Fact]
        public void Empty_Index() {
            //  Arrange
            var r = HalfOpenRangeList.New( 0L, -7L );
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
            var r = HalfOpenRangeList.New( 0L, 5L );
            //  Act
            var n = r[2];
            //  Assert
            Assert.Equal( 5, r.Count );
            Assert.Equal( 2L, n );
            for (long i = 0L; i < 5L; i++) {
                Assert.True( r.Contains( i ) );
            }
            Assert.False( r.Contains( -1L ) );
            Assert.Equal( -1L, r.IndexOf( 6L ) );
            Assert.Equal( 2, r.IndexOf( 2L ) );
        }

        [Fact]
        public void Empty_Enumerate() {
            //  Arrange
            var r = HalfOpenRangeList.New( 0L, -7L );
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
            var r = HalfOpenRangeList.New( 10L, 15L );
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

        [Fact]
        public void Indexing() {
            //  Arrange
            var r = HalfOpenRangeList.New( 10L, 15L );
            //  Act
            //  Assert
            Assert.ThrowsAny<NutmegException>( () => r[8] );
        }
        [Fact]
        public void IndexOf() {
            //  Arrange
            var r = HalfOpenRangeList.New( 10L, 15L );
            //  Act
            var i10 = r.IndexOf( 10L );
            var i11 = r.IndexOf( 11L );
            var i12 = r.IndexOf( 12L );
            var i13 = r.IndexOf( 13L );
            var i14 = r.IndexOf( 14L );
            var im1 = r.IndexOf( 77L );
            //  Assert
            Assert.Equal( 0, i10 );
            Assert.Equal( 1, i11 );
            Assert.Equal( 2, i12 );
            Assert.Equal( 3, i13 );
            Assert.Equal( 4, i14 );
            Assert.Equal( 0, i10 );
            Assert.Equal( -1, im1 );
        }

        [Fact]
        public void Step2_Typical() {
            //  Arrange
            var r = HalfOpenRangeList.New( 10L, 15L, step: 2L );
            //  Act
            var e = r.GetEnumerator();
            bool b0 = e.MoveNext();
            var e0 = e.Current;
            bool b1 = e.MoveNext();
            var e1 = e.Current;
            bool b2 = e.MoveNext();
            var e2 = e.Current;
            bool b3 = e.MoveNext();
            //  Assert
            Assert.Equal( 3, r.Count );
            Assert.Throws<NutmegException>( () => r[-1] );
            Assert.Equal( 10L, r[0] );
            Assert.Equal( 12L, r[1] );
            Assert.Equal( 14L, r[2] );
            Assert.Throws<NutmegException>( () => r[3] );
            Assert.True( b0 );
            Assert.Equal( 10L, e0 );
            Assert.True( b1 );
            Assert.Equal( 12L, e1 );
            Assert.True( b2 );
            Assert.Equal( 14L, e2 );
            Assert.False( b3 );
        }

        [Fact]
        public void Step2_IndexOf_Typical() {
            //  Arrange
            var r = HalfOpenRangeList.New( 10L, 15L, step: 2L );
            //  Act
            var i10 = r.IndexOf( 10L );
            var i12 = r.IndexOf( 12L );
            var i14 = r.IndexOf( 14L );
            //  Assert
            Assert.Equal( 0, i10 );
            Assert.Equal( 1, i12 );
            Assert.Equal( 2, i14 );
        }

        [Fact]
        public void Step2_Empty() {
            //  Arrange
            var r = HalfOpenRangeList.New( 10L, 10L, step: 2L );
            //  Act
            var e = r.GetEnumerator();
            bool b0 = e.MoveNext();
            //  Assert
            Assert.Throws<NutmegException>( () => r[-1] );
            Assert.Empty( r );
            Assert.Throws<NutmegException>( () => r[0] );
            Assert.False( b0 );
        }

        [Fact]
        public void Step2_OneItem() {
            //  Arrange
            var r = HalfOpenRangeList.New( 10L, 12L, step: 2L );
            //  Act
            var e = r.GetEnumerator();
            bool b0 = e.MoveNext();
            var e0 = e.Current;
            bool b1 = e.MoveNext();
            //  Assert
            Assert.Throws<NutmegException>( () => r[-1] );
            Assert.Equal( 10L, r[0] );
            Assert.Throws<NutmegException>( () => r[1] );
            Assert.True( b0 );
            Assert.Equal( 10L, e0 );
            Assert.False( b1 );
        }

        [Fact]
        public void Step0() {
            //  Assert
            Assert.Throws<NutmegException>( () => HalfOpenRangeList.New( 10L, 12L, step: 0L ) );
        }

        [Fact]
        public void StepM1_OneItem_IndexOf() {
            //  Arrange
            var r = HalfOpenRangeList.New( 4L, 0L, step: -1L );
            //  Act
            var i4 = r.IndexOf( 4L );
            var i3 = r.IndexOf( 3L );
            var i2 = r.IndexOf( 2L );
            var i1 = r.IndexOf( 1L );
            var i0 = r.IndexOf( 99L );
            //  Arrange
            Assert.Equal( 0, i4 );
            Assert.Equal( 1, i3 );
            Assert.Equal( 2, i2 );
            Assert.Equal( 3, i1 );
            Assert.Equal( -1, i0 );
        }

        [Fact]
        public void StepM1_OneItem() {
            //  Arrange
            var r = HalfOpenRangeList.New( 4L, 0L, step: -1L );
            //  Act
            var e = r.GetEnumerator();
            bool b0 = e.MoveNext();
            var e0 = e.Current;
            bool b1 = e.MoveNext();
            var e1 = e.Current;
            bool b2 = e.MoveNext();
            var e2 = e.Current;
            bool b3 = e.MoveNext();
            var e3 = e.Current;
            bool b4 = e.MoveNext();
            //  Assert
            Assert.Equal( 4, r.Count );
            Assert.Throws<NutmegException>( () => r[-1] );
            Assert.Equal( 4L, r[0] );
            Assert.Equal( 3L, r[1] );
            Assert.Equal( 2L, r[2] );
            Assert.Equal( 1L, r[3] );
            Assert.Throws<NutmegException>( () => r[4] );
            Assert.True( b0 );
            Assert.Equal( 4L, e0 );
            Assert.True( b1 );
            Assert.Equal( 3L, e1 );
            Assert.True( b2 );
            Assert.Equal( 2L, e2 );
            Assert.True( b3 );
            Assert.Equal( 1L, e3 );
            Assert.False( b4 );
        }


    }

}
