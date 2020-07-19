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
        [DataTestMethod]
        [DataRow("GET", "GET", HttpMethod.Type.Get)]
        [DataRow("HEAD", "HEAD", HttpMethod.Type.Head)]
        [DataRow("POST", "POST", HttpMethod.Type.Post)]
        [DataRow("PUT", "PUT", HttpMethod.Type.Put)]
        [DataRow("DELETE", "DELETE", HttpMethod.Type.Delete)]
        [DataRow("TRACE", "TRACE", HttpMethod.Type.Trace)]
        [DataRow("CONNECT", "CONNECT", HttpMethod.Type.Connect)]
        [DataRow("OPTIONS", "OPTIONS", HttpMethod.Type.Options)]
        public void FromString_ValidMethod_ReturnValidMethodObject(string encodedMethod,
            string expectedEncodedMethod,
            HttpMethod.Type expectedHttpMethodType)
        {
            // Act
            var method = HttpMethod.FromString(encodedMethod);

            // Assert
            Assert.AreEqual(expectedEncodedMethod, method.EncodedMethod);
            Assert.AreEqual(expectedHttpMethodType, method.MethodType);
        }

        [TestMethod]
        public void FromString_MixedCase_ReturnValidMethodObject()
        {
            // Act
            var getMethod = HttpMethod.FromString("gEt");
            var deleteMethod = HttpMethod.FromString("deLETE");

            // Assert
            Assert.AreEqual("GET", getMethod.EncodedMethod);
            Assert.AreEqual(HttpMethod.Type.Delete, deleteMethod.MethodType);
        }

        [TestMethod]
        public void FromString_InvalidEncodedMethod_ReturnsNull()
        {
            // Act
            var method = HttpMethod.FromString("unknown");

            // Assert
            Assert.IsNull(method);
        }

        [TestMethod]
        public void FromString_WithUntrimmedWhitespaces_ReturnsValidMethodObject()
        {
            // Act
            var method = HttpMethod.FromString("  GET         ");

            // Assert
            Assert.AreEqual(HttpMethod.Type.Get, method.MethodType);
        }

        [TestMethod]
        public void FromString_ParameterIsNull_ReturnsNull()
        {
            // Act
            var method = HttpMethod.FromString(null);

            // Assert
            Assert.IsNull(method);
        }

        [TestMethod]
        public void FromString_ParameterIsEmpty_ReturnsNull()
        {
            // Act
            var method = HttpMethod.FromString(string.Empty);

            // Assert
            Assert.IsNull(method);
        }

        [TestMethod]
        public void FromString_ParameterIsOnlyWhiteSpace_ReturnsNull()
        {
            // Act
            var method = HttpMethod.FromString("      \t     \n    \r    ");

            // Assert
            Assert.IsNull(method);
        }
    }
}
