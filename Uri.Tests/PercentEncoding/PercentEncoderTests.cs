#region Copyrights
// This file is a part of the Uri.Tests.
//
// Copyright (c) 2020 Kamil Rusin
// Licensed under the MIT License.
// See LICENSE.txt file in the project root for full license information.
#endregion

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uri.PercentEncoding;

namespace Uri.Tests.PercentEncoding
{
    [TestClass]
    public class PercentEncoderTests
    {
        [TestMethod]
        public void Encode_GivenEmptyString_ReturnsEmptyString()
        {
            // Act
            var encoded = PercentEncoder.Encode(string.Empty);

            // Assert
            Assert.AreEqual(string.Empty, encoded);
        }

        [TestMethod]
        public void Encode_GivenNull_ReturnsNull()
        {
            // Act
            var encoded = PercentEncoder.Encode(null);

            // Assert
            Assert.IsNull(encoded);
        }

        [TestMethod]
        public void Encode_GivenSegmentWithNoReservedChars_ReturnsTheSameSegment()
        {
            // Act
            var encoded = PercentEncoder.Encode("example.com");

            // Assert
            Assert.AreEqual("example.com", encoded);
        }

        [TestMethod]
        public void Encode_GivenSegmentWithOneReservedChar_ReturnsEncodedSegment()
        {
            // Act
            var encoded = PercentEncoder.Encode("ex ample.com");

            // Assert
            Assert.AreEqual("ex%20ample.com", encoded);
        }

        [TestMethod]
        public void Encode_GivenSegmentWithOnlySpaces_ReturnsEncodedSegment()
        {
            // Act
            var encoded = PercentEncoder.Encode("      ");

            // Assert
            Assert.AreEqual("%20%20%20%20%20%20", encoded);
        }

        [TestMethod]
        public void Encode_GivenSegmentWithPercentChars_ReturnsEncodedSegment()
        {
            // Act
            var encoded = PercentEncoder.Encode("%26%26%26%26%26%26");

            // Assert
            Assert.AreEqual("%2526%2526%2526%2526%2526%2526", encoded);
        }

        [DataTestMethod]
        [DataRow("exam/[ple#].com", "exam%2F%5Bple%23%5D.com")]
        public void Encode_GivenSegmentWithOneReservedChars_ReturnsEncodedSegment(
            string segment,
            string encodedSegment
        )
        {
            // Act
            var encoded = PercentEncoder.Encode(segment);

            // Assert
            Assert.AreEqual(encodedSegment, encoded);
        }

        [TestMethod]
        public void Encode_GivenSegmentAndHashSetOfAllowedChars_ReturnsEncodedSegment()
        {
            // Arrange
            var allowedChars = new HashSet<char> { '#' };

            // Act
            var encoded = PercentEncoder.Encode("example:#.com", allowedChars);

            // Assert
            Assert.AreEqual("example%3A#.com", encoded);
        }
    }
}
