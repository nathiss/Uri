#region Copyrights
// This file is a part of the Uri.
//
// Copyright (c) 2020 Kamil Rusin
// Licensed under the MIT License.
// See LICENSE.txt file in the project root for full license information.
#endregion

using System.Collections.Generic;
using System.Linq;

namespace Uri.PercentEncoding
{
    /// <summary>
    /// This class provider the functionality to percent-encode a segment of an URI.
    /// It's used by <see cref="Uri" />.
    /// </summary>
    public static class PercentEncoder
    {
        /// <summary>
        /// This method performs percent-encoding of the given <paramref name="segment" /> and
        /// returns a string containing the percent-encoded representation of it.
        /// </summary>
        /// <param name="segment">
        /// This is the string on which percent-encoding will be performed.
        /// </param>
        /// <returns>
        /// A string containing the percent-encoded representation of <paramref name="segment" />
        /// is returned.
        /// </returns>
        public static string Encode(string segment)
        {
            return Encode(segment, null);
        }

        /// <summary>
        /// This method performs percent-encoding of the given <paramref name="segment" /> and
        /// returns a string containing the percent-encoded representation of it.
        /// </summary>
        /// <param name="segment">
        /// This is the string on which percent-encoding will be performed.
        /// </param>
        /// <param name="allowedCharacters">
        /// This is a collection of characters allowed in the segment. These characters will not be
        /// percent-encoded.
        /// </param>
        /// <returns>
        /// A string containing the percent-encoded representation of <paramref name="segment" />
        /// is returned.
        /// </returns>
        public static string Encode(string segment, HashSet<char> allowedCharacters)
        {
            return Encode(segment, allowedCharacters, null);
        }

        /// <summary>
        /// This method performs percent-encoding of the given <paramref name="segment" /> and
        /// returns a string containing the percent-encoded representation of it.
        /// </summary>
        /// <param name="segment">
        /// This is the string on which percent-encoding will be performed.
        /// </param>
        /// <param name="allowedCharacters">
        /// This is a collection of characters allowed in the segment. These characters will not be
        /// percent-encoded.
        /// </param>
        /// <param name="disallowedCharacters">
        /// This is a collection of characters not allowed in the segment. These characters will be
        /// percent-encoded.
        /// </param>
        /// <returns>
        /// A string containing the percent-encoded representation of <paramref name="segment" />
        /// is returned.
        /// </returns>
        public static string Encode(string segment, HashSet<char> allowedCharacters, HashSet<char> disallowedCharacters)
        {
            if (string.IsNullOrEmpty(segment))
            {
                return segment;
            }

            var reservedCharactersMap = ReservedCharacters
                .Except(allowedCharacters ?? new HashSet<char>())
                .Union(disallowedCharacters ?? new HashSet<char>())
                .Select(chr => new KeyValuePair<string, string>(
                    chr.ToString(),
                    $"%{(int)chr:X}"
                ));

            foreach (var reservedCharacterMap in reservedCharactersMap)
            {
                segment = segment.Replace(reservedCharacterMap.Key, reservedCharacterMap.Value);
            }

            return segment;
        }

        /// <summary>
        /// This is the collection of reserved characters defined in
        /// <see href="https://tools.ietf.org/html/rfc3986#section-2.2">RFC 3986 (Section 2.2)</see>.
        /// </summary>
        /// <value>
        /// This field is used to generate the value of <see cref="ReservedCharactersMap" />.
        /// </value>
        private static readonly HashSet<char> ReservedCharacters = new HashSet<char>
        {
            // Those two characters are not defined as "reserved" by RFC 3986, but their must be
            // percent-encoded to ensure valid representation of URIs.
            '%', ' ',

            ':', '/', '?', '#', '[' , ']', '@',

            '*', '+', ',', ';', '=',
        };
    }
}
