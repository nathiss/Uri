#region Copyrights
// This file is a part of the SharpShare.
//
// Copyright (c) 2020 Kamil Rusin
// Licensed under the MIT License.
// See LICENSE.txt file in the project root for full license information.
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpShare.Core.Schematics;

namespace SharpShare.Core.Tests.Schematics
{
    [TestClass]
    public class HttpMethodTests
    {
        [TestMethod]
        [DataRow("GET", "GET", HttpMethodType.Get)]
        [DataRow("HEAD", "HEAD", HttpMethodType.Head)]
        [DataRow("POST", "POST", HttpMethodType.Post)]
        [DataRow("PUT", "PUT", HttpMethodType.Put)]
        [DataRow("DELETE", "DELETE", HttpMethodType.Delete)]
        [DataRow("TRACE", "TRACE", HttpMethodType.Trace)]
        [DataRow("CONNECT", "CONNECT", HttpMethodType.Connect)]
        [DataRow("OPTIONS", "OPTIONS", HttpMethodType.Options)]
        public void FromString_ValidMethod_ReturnValidMethodObject(string encodedMethod,
            string expectedEncodedMethod,
            HttpMethodType expectedHttpMethodType)
        {
            // Act
            var method = HttpMethod.FromString(encodedMethod);

            // Assert
            Assert.AreEqual(expectedEncodedMethod, method.EncodedMethod);
            Assert.AreEqual(expectedHttpMethodType, method.HttpMethodType);
        }

        [TestMethod]
        public void FromString_MixedCase_ReturnValidMethodObject()
        {
            // Act
            var getMethod = HttpMethod.FromString("gEt");
            var deleteMethod = HttpMethod.FromString("deLETE");

            // Assert
            Assert.AreEqual("GET", getMethod.EncodedMethod);
            Assert.AreEqual(HttpMethodType.Delete, deleteMethod.HttpMethodType);
        }

        [TestMethod]
        public void FromString_InvalidEncodedMethod_ReturnInvalidMethodObject()
        {
            // Act
            var method = HttpMethod.FromString("unknown");

            // Assert
            Assert.AreEqual("INVALID", method.EncodedMethod);
            Assert.AreEqual(HttpMethodType.Invalid, method.HttpMethodType);
        }

        [TestMethod]
        public void FromString_WithUntrimmedWhitespaces_ReturnsValidMethodObject()
        {
            // Act
            var method = HttpMethod.FromString("  GET         ");

            // Assert
            Assert.AreEqual(HttpMethodType.Get, method.HttpMethodType);
        }
    }
}
