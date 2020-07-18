#region Copyrights
// This file is a part of the SharpShare.
//
// Copyright (c) 2020 Kamil Rusin
// Licensed under the MIT License.
// See LICENSE.txt file in the project root for full license information.
#endregion

using System.Linq;

namespace SharpShare.Core.Utils
{
    /// <summary>
    /// This class contains extension methods for <see cref="string"/> class.
    /// </summary>
    public static class StringExtensions
    {
        private const int MaxAnsiCode = 255;

        private const int MaxAnsiLetterCode = 127;

        public static bool ContainsNonAscii(this string str)
        {
            return str.Any(ch => ch > MaxAnsiCode);
        }

        public static bool ContainsNonAsciiLetters(this string str)
        {
            return str.Any(ch => ch > MaxAnsiLetterCode);
        }
    }
}
