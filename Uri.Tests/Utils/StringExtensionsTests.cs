#region Copyrights
// This file is a part of the Uri.Tests.
//
// Copyright (c) 2020 Kamil Rusin
// Licensed under the MIT License.
// See LICENSE.txt file in the project root for full license information.
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Uri.Utils.Tests
{
    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void ContainsNonAscii_OnlyAsciiChars_ReturnsFalse()
        {
            Assert.IsFalse("THisIn only ascii !!!! text @@ ^&.".ContainsNonAscii());
        }

        [TestMethod]
        public void ContainsNonAscii_EmptyString_ReturnsFalse()
        {
            Assert.IsFalse("".ContainsNonAscii());
        }

        [TestMethod]
        public void ContainsNonAscii_ContainsNonAsciiChars_ReturnsTrue()
        {
            Assert.IsTrue("ҁҭӖ".ContainsNonAscii());
        }

        [TestMethod]
        public void ContainsNonAscii_ContainsOneNonAsciiChar_ReturnsTrue()
        {
            Assert.IsTrue("THisIn only ascii Ʋ!!!! text @@ ^&.".ContainsNonAscii());
        }

        [TestMethod]
        public void ContainsNonAsciiLetters_OnlyAscii_ReturnsFalse()
        {
            Assert.IsFalse("THisIn only ascii !!!! text @@ ^&.".ContainsNonAsciiLetters());
        }

        [TestMethod]
        public void ContainsNonAsciiLetters_EmptyString_ReturnsFalse()
        {
            Assert.IsFalse("".ContainsNonAsciiLetters());
        }

        [TestMethod]
        public void ContainsNonAsciiLetters_ContainsNonAsciiLetters_ReturnsTrue()
        {
            Assert.IsTrue("ąä Test".ContainsNonAsciiLetters());
        }

        [TestMethod]
        public void ContainsNonAsciiLetters_ContainsNonAsciiLetter_ReturnsTrue()
        {
            Assert.IsTrue("THisIn only ascii ä!!!! text @@ ^&.".ContainsNonAsciiLetters());
        }
    }
}
