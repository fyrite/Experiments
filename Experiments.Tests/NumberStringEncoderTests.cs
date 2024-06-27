using System;
using NUnit.Framework;

namespace Experiments.Tests
{
    [TestFixture]
    public class NumberStringEncoderTests
    {
        [TestCase("10", 1)]
        [TestCase("16", 1)]
        [TestCase("10215", 2)] // original length = 5
        [TestCase("615354", 3)] // original length = 6
        [TestCase("1101215", 2)] // original length = 7
        [TestCase("1928374", 4)] // original length = 7
        [TestCase("1011121314", 3)] // original length = 10
        [TestCase("1928374650", 5)] // original length = 10
        public void Encode_IncludesHeaderByte(string input, int encodedLength)
        {
            // Arrange
            const int headerLength = 1;

            // Act
            var encoded = NumberStringEncoder.Encode(input);

            // Assert
            Assert.That(encoded.Length - headerLength, Is.EqualTo(encodedLength));
        }

        [TestCase("10", 1)]
        [TestCase("16", 1)]
        [TestCase("10215", 2)] // original length = 5
        [TestCase("615354", 3)] // original length = 6
        [TestCase("1101215", 2)] // original length = 7
        [TestCase("1928374", 4)] // original length = 7
        [TestCase("1011121314", 3)] // original length = 10
        [TestCase("1928374650", 5)] // original length = 10
        public void EncodedLength_IsLessThanOrEqualTo_RoughlyHalfOriginalLength_ExcludingHeaderByte(string input,
            int encodedLength)
        {
            // Arrange
            const int headerLength = 1;
            var halfOriginalLength = Math.Ceiling(input.Length * 0.5);

            // Act
            var encoded = NumberStringEncoder.Encode(input);

            // Assert
            Assert.That(encoded.Length - headerLength, Is.LessThanOrEqualTo(halfOriginalLength));
        }

        [TestCase("10")]
        [TestCase("16")]
        [TestCase("10215")]
        [TestCase("1101215")]
        public void EncodeDecode_ReturnsOriginalInput(string input)
        {
            // Arrange / Act
            var encoded = NumberStringEncoder.Encode(input);
            var decoded = NumberStringEncoder.Decode(encoded);

            // Assert
            Assert.That(decoded, Is.EqualTo(input));
        }

        [Test]
        public void Encode_ThrowsException_WithInvalidCharacters()
        {
            // Arrange / Act / Assert
            Assert.Throws<ArgumentException>(() => NumberStringEncoder.Encode("10abcABC!@#$%^&*()_+-=}{][:;<>?/.,"));
        }

        [TestCase("16", "16")]
        [TestCase("10215", "A2F")]
        [TestCase("1101215", "1ACF")]
        [TestCase("101112131415", "ABCDEF")]
        [TestCase("1011-hello-12131415*", "AB-hello-CDEF*")]
        public void ToSudoHex_ReplacesNumbersGreaterThan10AndLessThan16(string input, string output)
        {
            // Arrange / Act
            var sudoHex = NumberStringEncoder.ToSudoHex(input);

            // Assert
            Assert.That(sudoHex, Is.EqualTo(output));
        }

        [TestCase("10215")]
        public void SudoHex_DoesNotAlwaysEquateToHex(string input)
        {
            // Arrange / Act
            var sudoHex = NumberStringEncoder.ToSudoHex(input);

            // Assert
            var hex = Convert.ToInt32(input).ToString("X");
            Assert.That(sudoHex, Is.Not.EqualTo(hex));
        }
    }
}