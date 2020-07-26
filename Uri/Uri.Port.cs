#region Copyrights
// This file is a part of the Uri.
//
// Copyright (c) 2020 Kamil Rusin
// Licensed under the MIT License.
// See LICENSE.txt file in the project root for full license information.
#endregion

namespace Uri
{
    public partial class Uri
    {
        /// <summary>
        /// This property represents the Port of a URI. If the port component was not present
        /// in the URI, the value of this property will be -1.
        /// </summary>
        /// <value>
        /// If the Port component of the URI is present but empty, the value of this property
        /// is zero.
        /// </value>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.2.2">RFC 3986 (Section 3.2.2)</seealso>
        public int Port { get; private set; }

        /// <summary>
        /// This property returns an indication of whether or not the URI has a Port component.
        /// </summary>
        public bool HasPort => Port != -1;

        /// <summary>
        /// This method parses the given <paramref name="authority"/> and returns the port of
        /// the URI, if the URI does not have the authority component this method will return
        ///  -1. If the Port component of the URI is empty, this method will return 0.
        /// </summary>
        /// <param name="authority">
        /// This is the authority component of the URI. If the authority is empty, then
        /// the port component is not present in the URI.
        /// </param>
        /// <param name="offset">
        /// This is the offset of the given <paramref name="authority"/> after which the
        /// lookout will be performed. If the offset is greater or equal to the length
        /// of <paramref name="authority"/> and the URI has the authority component,
        /// this method will return -1.
        /// </param>
        /// <returns>
        /// A integer representing the port component of the URI is returned.
        /// </returns>
        /// <exception cref="InvalidUriException">
        /// If the port component of the URI contains non-digit character an exception of
        /// this type is thrown.
        /// </exception>
        private static int GetPortComponent(string authority, int offset)
        {
            if (string.IsNullOrWhiteSpace(authority))
            {
                return -1;
            }

            if (offset >= authority.Length)
            {
                if (authority[offset - 1] == ':')
                {
                    return 0;
                }

                return -1;
            }

            var portString = authority.Substring(offset);

            if (!int.TryParse(portString, out var port))
            {
                throw new InvalidUriException();
            }

            return port;
        }
    }
}
