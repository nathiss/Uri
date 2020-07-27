#region Copyrights
// This file is a part of the Uri.
//
// Copyright (c) 2020 Kamil Rusin
// Licensed under the MIT License.
// See LICENSE.txt file in the project root for full license information.
#endregion

using System;
using System.Text;

namespace Uri
{
    public partial class Uri
    {
        /// <summary>
        /// This property represents the Authority of a URI. If the authority component was
        /// not present in the URI, the value of this property will be null.
        /// </summary>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.2">RFC 3986 (Section 3.2)</seealso>
        public string Authority { get; private set; }

        /// <summary>
        /// This property returns an indication of whether or not the URI has an Authority component.
        /// </summary>
        public bool HasAuthority => Authority != null;

        /// <summary>
        /// This method parses the given <paramref name="uriString"/> and returns the
        /// authority of the URI, if the URI does not have an authority this method
        /// returns a pair of null and an offset equal to the given <paramref name="offset"/>.
        /// </summary>
        /// <param name="uriString">
        /// This is the URI encoded as string. This parameter must not be null.
        /// </param>
        /// <param name="offset">
        /// This is the offset of the given <paramref name="uriString"/> after which
        /// the lookout will be performed. If the offset is greater or equal to
        /// <paramref name="uriString"/> Length, this method will return a pair of
        /// null and the given offset.
        /// </param>
        /// <returns>
        /// A pair of a lowercase string representing the authority and the offset of the rest
        /// of <paramref name="uriString"/> is returned. If the URI does not have an authority
        /// the method returns a pair of null and an offset which is equal to the given
        /// <paramref name="offset"/>.
        /// </returns>
        /// <remarks>
        /// This method does not check if the authority component of the URI has only permitted
        /// characters. Though <see cref="GetUserInformationComponent"/>, <see cref="GetHostComponent"/> and
        /// <see cref="GetPortComponent"/> does so for their components.
        /// </remarks>
        private static (string Authority, int Offset) GetAuthorityComponent(string uriString, int offset)
        {
            try
            {
                var forwardSlashes = uriString.Substring(offset, 2);
                if (forwardSlashes != "//")
                {
                    return (null, offset);
                }

                var endOfAuthority = uriString.IndexOfAny(new []{'/', '?', '#'}, offset + 2);
                return endOfAuthority == -1
                    ? (uriString.Substring(offset + 2), uriString.Length)
                    : (uriString[(offset + 2)..endOfAuthority], endOfAuthority);
            }
            catch (ArgumentOutOfRangeException)
            {
                return (null, offset);
            }
        }

        /// <summary>
        /// This method converts the Authority component into a string and appends it into the
        /// given <paramref name="uriBuilder" /> <see cref="System.Text.StringBuilder" />.
        /// </summary>
        /// <param name="uriBuilder">
        /// This is the <see cref="System.Text.StringBuilder" /> into which the Authority
        /// component will be added.
        /// </param>
        private void AuthorityToString(StringBuilder uriBuilder)
        {
            if (Authority != null)
            {
                uriBuilder.Append($"//{Authority}");
            }
        }
    }
}
