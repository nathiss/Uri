#region Copyrights
// This file is a part of the Uri.
//
// Copyright (c) 2020 Kamil Rusin
// Licensed under the MIT License.
// See LICENSE.txt file in the project root for full license information.
#endregion

using System.Text;
using Uri.Exceptions;

namespace Uri
{
    /// <summary>
    /// This class represents the Port component of a URI.
    /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.2.3">RFC 3986 (Section 3.2.3)</seealso>
    /// </summary>
    internal class PortComponent
    {
        /// <summary>
        /// This property represents the Port of a URI. If the port component was not present
        /// in the URI, the value of this property will be -1.
        /// </summary>
        /// <value>
        /// If the Port component of the URI is present but empty, the value of this property
        /// is zero.
        /// </value>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.2.3">RFC 3986 (Section 3.2.3)</seealso>
        internal int Port { get; }

        /// <summary>
        /// This method returns a string representation of the Port component.
        /// </summary>
        /// <returns>
        /// A string representation of the Port component is returned.
        /// </returns>
        public override string ToString() => Port.ToString();

        /// <summary>
        /// This method returns an indication of whether or not the URI has the Port component.
        /// </summary>
        /// <returns>
        /// An indication of whether or not the URI has the Port component is returned.
        /// </returns>
        internal bool HasPort => Port != 0;

        /// <summary>
        /// This constructor sets the given <paramref name="port" /> as the port value of this
        /// instance.
        /// </summary>
        /// <param name="port">
        /// This is the port number from the URI. Value -1 indicates that the URI does not have the Port
        /// component.
        /// </param>
        private PortComponent(int port)
        {
            Port = port;
        }

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
        /// A <see cref="PortComponent" /> object representing the port component of the URI is returned.
        /// </returns>
        /// <exception cref="InvalidUriException">
        /// If the port component of the URI contains non-digit character an exception of
        /// this type is thrown.
        /// </exception>
        internal static PortComponent FromString(string authority, int offset)
        {
            if (string.IsNullOrWhiteSpace(authority))
            {
                return null;
            }

            if (offset >= authority.Length)
            {
                if (authority[offset - 1] == ':')
                {
                    return new PortComponent(0);
                }

                return null;
            }

            var portString = authority.Substring(offset);

            if (!int.TryParse(portString, out var port))
            {
                throw new InvalidUriException();
            }

            return new PortComponent(port);
        }

        /// <summary>
        /// This method converts the Port component into a string and appends it into the
        /// given <paramref name="uriBuilder" /> <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="uriBuilder">
        /// This is the <see cref="StringBuilder" /> into which the Port component will be added.
        /// </param>
        internal void BuildEncodedString(StringBuilder uriBuilder)
        {
            if (HasPort)
            {
                uriBuilder.Append(':').Append(ToString());
            }
        }
    }
}
