#region Copyrights
// This file is a part of the Uri.
//
// Copyright (c) 2020 Kamil Rusin
// Licensed under the MIT License.
// See LICENSE.txt file in the project root for full license information.
#endregion

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uri.Exceptions;
using Uri.PercentEncoding;

namespace Uri
{
    /// <summary>
    /// This class represents the Fragment component of a URI.
    /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.5">RFC 3986 (Section 3.5)</seealso>
    /// </summary>
    internal class FragmentComponent
    {
        /// <summary>
        /// This property holds a string representation of the Fragment component of an URI.
        /// </summary>
        internal string Fragment { get; }

        /// <summary>
        /// This constructor sets the value of <see cref="Fragment" /> property.
        /// </summary>
        /// <param name="fragmentString">
        /// This is a string representation of the Fragment component of an URI.
        /// </param>
        private FragmentComponent(string fragmentString)
        {
            Fragment = fragmentString;
        }

        /// <summary>
        /// This method parses the given <paramref name="uriString"/> and returns the Fragment of
        /// the URI, if the URI does not have the Fragment component this method will return null.
        /// If the Fragment component of the URI is empty, this method will return
        /// <see cref="string.Empty"/>.
        /// </summary>
        /// <param name="uriString">
        /// This is the string representation of the URI. This parameter must not be null.
        /// </param>
        /// <param name="offset">
        /// This is the offset of the given <paramref name="uriString"/> after which the
        /// lookout will be performed. If the offset is greater or equal to the length
        /// of <paramref name="uriString"/> this method will return null.
        /// </param>
        /// <returns>
        /// A <see cref="FragmentComponent" /> object is returned.
        /// </returns>
        /// <exception cref="InvalidUriException">
        /// If the Fragment component contains not allowed characters an exception of this type
        /// is thrown.
        /// </exception>
        internal static FragmentComponent FromString(string uriString, int offset)
        {
            if (offset >= uriString.Length)
            {
                return null;
            }

            if (uriString[offset] != '#')
            {
                return null;
            }

            var targetString = uriString.Substring(offset + 1);

            if (!targetString.All(ch => FragmentAllowedCharacters.Contains(ch)))
            {
                throw new InvalidUriException();
            }

            return new FragmentComponent(PercentDecoder.Decode(targetString));
        }

        /// <summary>
        /// This collection contains all possible characters that can be used inside a Fragment
        /// component.
        /// </summary>
        private static readonly HashSet<char> FragmentAllowedCharacters = new HashSet<char>
        {
            // PCHAR

            // Unreserved

            // Alpha lowercase
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',

            // Alpha uppercase
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',

            // Digit
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',

            '-', '.', '_', '~',

            // Percent-Encoded

            '%', 'A', 'B', 'C', 'D', 'E', 'F',

            // sub-delims

            '!', '$', '&', '\'', '(', ')', '*', '+', ',', ';', '=',

            '/', '?',
        };


        /// <summary>
        /// This method converts the Fragment component into a string and appends it into the
        /// given <paramref name="uriBuilder" /> <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="uriBuilder">
        /// This is the <see cref="StringBuilder" /> into which the Fragment component will be added.
        /// </param>
        internal void BuildEncodedString(StringBuilder uriBuilder)
        {
            if (!string.IsNullOrEmpty(Fragment))
            {
                uriBuilder.Append('#').Append(PercentEncoder.Encode(Fragment));
            }
        }
    }
}
