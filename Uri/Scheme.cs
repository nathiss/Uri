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

namespace Uri
{
    /// <summary>
    /// This class represents the Scheme component of a URI.
    /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.1">RFC 3986 (Section 3.1)</seealso>
    /// </summary>
    internal class SchemeComponent
    {
        /// <summary>
        /// This property represents the Scheme of a URI.
        /// </summary>
        /// <remarks>
        /// The property contains a lowercase representation of the scheme.
        /// </remarks>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.1">RFC 3986 (Section 3.1)</seealso>
        internal string Scheme { get; }


        /// <summary>
        /// This constructor sets the value of <see cref="Scheme" /> to the given <paramref name="schemeString" />.
        /// </summary>
        /// <param name="schemeString">
        /// This is a string representation of the Scheme component of the URI.
        /// </param>
        private SchemeComponent(string schemeString)
        {
            Scheme = schemeString;
        }

        /// <summary>
        /// This method parses the given <paramref name="uriString"/> and returns
        /// the Scheme of the URI and the offset of the rest of <paramref name="uriString"/>.
        /// </summary>
        /// <param name="uriString">
        /// This is the URI encoded as string. This parameter must not be null.
        /// </param>
        /// <returns>
        /// A pair of an <see cref="SchemeComponent" /> object and the
        /// offset of the reset of <paramref name="uriString"/> is returned.
        /// if the URI does not contain the Scheme, a pair of null and 0 is returned.
        /// </returns>
        internal static (SchemeComponent Scheme, int Offset) FromString(string uriString)
        {
            if (uriString.Length == 0 || !char.IsLetter(uriString[0]))
            {
                return (null, 0);
            }

            var delimiterIndex = uriString.IndexOf(':');
            if (delimiterIndex == -1)
            {
                return (null, 0);
            }

            var scheme = uriString.Substring(0, delimiterIndex).ToLower();

            if (!scheme.All(ch => SchemeAllowedCharacters.Contains(ch)))
            {
                throw new InvalidUriException();
            }

            return (new SchemeComponent(scheme), delimiterIndex + 1);
        }

        /// <summary>
        /// This collection contains all possible characters that can be used inside a URI Scheme
        /// component.
        /// </summary>
        private static HashSet<char> SchemeAllowedCharacters { get; } = new HashSet<char>
        {
            // ASCII letters
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l','m',
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',

            // ASCII decimal digits
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',

            // ASCII special characters
            '+', '-' ,'.',
        };

        /// <summary>
        /// This method converts the Scheme component into a string and appends it into the
        /// given <paramref name="uriBuilder" /> <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="uriBuilder">
        /// This is the <see cref="StringBuilder" /> into which the Scheme component will be added.
        /// </param>
        internal void BuildEncodedString(StringBuilder uriBuilder)
        {
            if (Scheme != null)
            {
                uriBuilder.Append(Scheme).Append(':');
            }
        }
    }
}
