#region Copyrights
// This file is a part of the Uri.
//
// Copyright (c) 2020 Kamil Rusin
// Licensed under the MIT License.
// See LICENSE.txt file in the project root for full license information.
#endregion

using System.Linq;

namespace Uri.Utils
{
    /// <summary>
    /// This class contains extension methods for <see cref="string"/> class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// This is the maximum possible value that an ASCII character can have.
        /// </summary>
        private const int MaxAnsiCode = 255;

        /// <summary>
        /// This is the maximum possible value that an ASCII letter can have.
        /// </summary>
        private const int MaxAnsiLetterCode = 127;

        /// <summary>
        /// This method returns an indication of whether or not the given
        /// <paramref name="str"/> contains only ASCII characters.
        /// </summary>
        /// <param name="str">
        /// This is the string that search will be performed on.
        /// </param>
        /// <returns>
        /// An indication of whether or not the given <paramref name="str"/>
        /// contains only ASCII characters is returned.
        /// </returns>
        public static bool ContainsNonAscii(this string str)
        {
            return str.Any(ch => ch > MaxAnsiCode);
        }

        /// <summary>
        /// This method returns an indication of whether or not the given
        /// <paramref name="str"/> contains only ASCII letters.
        /// </summary>
        /// <param name="str">
        /// This is the string that search will be performed on.
        /// </param>
        /// <returns>
        /// An indication of whether or not the given <paramref name="str"/>
        /// contains only ASCII letters is returned.
        /// </returns>
        public static bool ContainsNonAsciiLetters(this string str)
        {
            return str.Any(ch => ch > MaxAnsiLetterCode);
        }
    }
}
