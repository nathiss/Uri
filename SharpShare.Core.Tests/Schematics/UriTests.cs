#region Copyrights
// This file is a part of the SharpShare.
//
// Copyright (c) 2020 Kamil Rusin
// Licensed under the MIT License.
// See LICENSE.txt file in the project root for full license information.
#endregion

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpShare.Core.Schematics;

namespace SharpShare.Core.Tests.Schematics
{
    [TestClass]
    public class UriTests
    {
        [DataTestMethod]
        [DataRow("https://example.com/", "https", "example.com", new[] {""})]
        [DataRow("https://www.example.com/", "https", "www.example.com", new[] {""})]
        [DataRow("https://www.example.com", "https", "www.example.com", new string[] {})]
        [DataRow("ssh://a.com/foo/bar", "ssh", "a.com", new [] {"", "foo", "bar"})]
        public void FromString_ReceivedValidUri_UriParsedProperly(string uriString, string scheme, string authority, string[] path)
        {
            // Arrange
            var pathList = path.ToList();

            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsNotNull(uri);
            Assert.AreEqual(scheme, uri.Scheme);
            Assert.AreEqual(authority, uri.Authority);
            Assert.IsTrue(pathList.SequenceEqual(uri.Path));
        }

        [TestMethod]
        public void FromString_ReceivedNull_ReturnedNull()
        {
            // Act
            var uri = Uri.FromString(null);

            // Assert
            Assert.IsNull(uri);
        }

        [TestMethod]
        public void FromString_ReceivedEmptyString_ReturnedNull()
        {
            // Act
            var uri = Uri.FromString(string.Empty);

            // Assert
            Assert.IsNull(uri);
        }

        [TestMethod]
        public void FromString_ReceivedStringContainingOnlyWhitespaces_ReturnedNull()
        {
            // Act
            var uri = Uri.FromString("    \t    \n    \r     ");

            // Assert
            Assert.IsNull(uri);
        }
    }
}
