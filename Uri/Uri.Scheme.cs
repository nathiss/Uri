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

namespace Uri
{
    public partial class Uri
    {
        /// <summary>
        /// This property represents the Scheme of a URI. The property will always contain
        /// non-empty string, since the scheme is a required component of the URI.
        /// </summary>
        /// <remarks>
        /// The property contains a lowercase representation of the scheme.
        /// </remarks>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.1">RFC 3986 (Section 3.1)</seealso>
        public string Scheme { get; private set; }

        /// <summary>
        /// This property returns an indication of whether or not the URI has a Scheme component.
        /// </summary>
        /// <value>
        /// The value of this property will always be true since the Scheme is a required
        /// component of the URI.
        /// </value>
        public bool HasScheme => Scheme != null;

        /// <summary>
        /// This method parses the given <paramref name="uriString"/> and returns
        /// the Scheme of the URI and the offset of the rest of <paramref name="uriString"/>.
        /// </summary>
        /// <param name="uriString">
        /// This is the URI encoded as string. This parameter must not be null.
        /// </param>
        /// <returns>
        /// A pair of a lowercase string representing the Scheme of the URI and the
        /// offset of the reset of <paramref name="uriString"/> is returned.
        /// if the URI does not contain the Scheme, which is an equivalent of being
        /// ill-formed, a pair of null and -1 is returned.
        /// </returns>
        private static (string Scheme, int Offset) GetSchemeComponent(string uriString)
        {
            if (uriString.Length == 0 || !char.IsLetter(uriString[0]))
            {
                return (null, -1);
            }

            var delimiterIndex = uriString.IndexOf(':');
            if (delimiterIndex == -1)
            {
                return (null, -1);
            }

            var scheme = uriString.Substring(0, delimiterIndex).ToLower();

            return scheme.All(ch => SchemeAllowedCharacters.Contains(ch))
                ? (scheme, delimiterIndex + 1) : (null, -1);
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
        private void BuildSchemeString(StringBuilder uriBuilder)
        {
            if (Scheme != null)
            {
                uriBuilder.Append(Scheme).Append(':');
            }
        }
    }
}
