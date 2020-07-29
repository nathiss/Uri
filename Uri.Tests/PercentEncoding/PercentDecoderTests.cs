#region Copyrights
// This file is a part of the Uri.Tests.
//
// Copyright (c) 2020 Kamil Rusin
// Licensed under the MIT License.
// See LICENSE.txt file in the project root for full license information.
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uri.Exceptions;
using Uri.PercentEncoding;

namespace Uri.Tests.PercentEncoding
{
    [TestClass]
    public class PercentDecoderTests
    {
        [TestMethod]
        public void Decode_GivenEmptyString_ReturnsEmptyString()
        {
            // Act
            var decoded = PercentDecoder.Decode(string.Empty);

            // Assert
            Assert.AreEqual(string.Empty, decoded);
        }

        [TestMethod]
        public void Decode_GivenNull_ReturnsNull()
        {
            // Act
            var decoded = PercentDecoder.Decode(null);

            // Assert
            Assert.IsNull(decoded);
        }

        [TestMethod]
        public void Decode_GivenSegmentWithNoPercentEncodedChars_ReturnsTheSameSegment()
        {
            // Act
            var decoded = PercentDecoder.Decode("example.com");

            // Assert
            Assert.AreEqual("example.com", decoded);
        }

        [TestMethod]
        public void Decode_GivenSegmentWithOnePercentEncodedChar_ReturnsDecodedSegment()
        {
            // Act
            var decoded = PercentDecoder.Decode("example%20.com");

            // Assert
            Assert.AreEqual("example .com", decoded);
        }

        [TestMethod]
        public void Decode_GivenSegmentWithOnePercentEncodedCharWithLowerCase_ReturnsDecodedSegment()
        {
            // Act
            var decoded = PercentDecoder.Decode("example%2f.com");

            // Assert
            Assert.AreEqual("example/.com", decoded);
        }

        [TestMethod]
        public void Decode_GivenSegmentWithOnePercentEncodedCharWithUpperCase_ReturnsDecodedSegment()
        {
            // Act
            var decoded = PercentDecoder.Decode("example%2F.com");

            // Assert
            Assert.AreEqual("example/.com", decoded);
        }

        [TestMethod]
        public void Decode_GivenSegmentWithManyPercentEncodedChars_ReturnsDecodedSegment()
        {
            // Act
            var decoded = PercentDecoder.Decode("%20ex%20ample%2f.%5bcom%5D");

            // Assert
            Assert.AreEqual(" ex ample/.[com]", decoded);
        }

        [TestMethod]
        public void Decode_GivenSegmentWithOnlyPercentEncodedChars_ReturnsDecodedSegment()
        {
            // Act
            var decoded = PercentDecoder.Decode("%20%5B%5C%5C%5D");

            // Assert
            Assert.AreEqual(" [\\\\]", decoded);
        }

        [TestMethod]
        public void Decode_GivenSegmentWithPercentSignPercentEncoded_ReturnsDecodedSegment()
        {
            // Act
            var decoded = PercentDecoder.Decode("example%2520.com");

            // Assert
            Assert.AreEqual("example%20.com", decoded);
        }

        [TestMethod]
        public void Decode_GivenSegmentWithWronglyPercentEncodedChars_ThrowsAnInvalidUriException()
        {
            // Assert
            Assert.ThrowsException<InvalidUriException>(() => PercentDecoder.Decode("example%2520.com%"));
        }
    }
}
