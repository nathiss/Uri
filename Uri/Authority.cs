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
    /// <summary>
    /// This class represents the Authority component of a URI.
    /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.2">RFC 3986 (Section 3.2)</seealso>
    /// </summary>
    internal class AuthorityComponent
    {
        /// <summary>
        /// This property represents the Authority of a URI. If the authority component was
        /// not present in the URI, the value of this property will be null.
        /// </summary>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.2">RFC 3986 (Section 3.2)</seealso>
        internal string Authority { get; private set; }

        /// <summary>
        /// This field represents the User Information of a URI. If the user information
        /// component was not present in the URI, the value of this field will be null.
        /// </summary>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.2.1">RFC 3986 (Section 3.2.1)</seealso>
        private readonly UserInformationComponent _userInformation;

        /// <summary>
        /// This method returns a string representation of the UserInformation component of the URI.
        /// </summary>
        internal string UserInformation => _userInformation.UserInformation;

        /// <summary>
        /// This method returns an indication of whether or not the URI has a UserInformation component.
        /// </summary>
        internal bool HasUserInformation => _userInformation != null;

        /// <summary>
        /// This field represents the Host of a URI. If the user information
        /// component was not present in the URI, the value of this field will be null.
        /// </summary>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.2.2">RFC 3986 (Section 3.2.2)</seealso>
        private readonly HostComponent _host;

        /// <summary>
        /// This method returns a string representation of the Host component of the URI.
        /// </summary>
        internal string Host => _host.Host;

        /// <summary>
        /// This method returns an indication of whether or not the URI has a Host component.
        /// </summary>
        internal bool HasHost => _host != null;

        /// <summary>
        /// This field represents the Port component of a URI. If the component was not present
        /// in the URI, then the value of this field is null.
        /// </summary>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.2.3">RFC 3986 (Section 3.2.3)</seealso>
        private readonly PortComponent _port;

        /// <summary>
        /// This method returns the Port of a URI. If the port component was not present
        /// in the URI, this method returns -1.
        /// </summary>
        /// <value>
        /// If the Port component of the URI is present but empty, this method returns zero.
        /// </value>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.2.3">RFC 3986 (Section 3.2.3)</seealso>
        internal int Port => _port?.Port ?? -1;

        /// <summary>
        /// This property returns an indication of whether or not the URI has a Port component.
        /// </summary>
        internal bool HasPort => _port?.HasPort ?? false;

        /// <summary>
        /// This constructor sets the value of <see cref="Authority" /> to the given
        /// <paramref name="authorityString" />.
        /// </summary>
        /// <param name="authorityString">
        /// This is a string representation of the Authority component of the URI.
        /// </param>
        private AuthorityComponent(string authorityString)
        {
            var authorityBuilder = new StringBuilder();

            var userInformation = UserInformationComponent.FromString(authorityString);
            _userInformation = userInformation.UserInformation;
            if (HasUserInformation)
            {
                authorityBuilder.Append(UserInformation).Append('@');
            }

            var host = HostComponent.FromString(authorityString, userInformation.Offset);
            _host = host.Host;
            if (host.IsIpLiteral)
            {
                authorityBuilder.Append('[').Append(Host).Append(']');
            }
            else
            {
                authorityBuilder.Append(Host);
            }

            _port = PortComponent.FromString(authorityString, host.Offset);
            if (HasPort)
            {
                authorityBuilder.Append(':').Append(Port);
            }

            Authority = authorityBuilder.ToString();
        }

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
        /// A pair of an <see cref="AuthorityComponent" /> object and the offset of the rest
        /// of <paramref name="uriString"/> is returned. If the URI does not have an authority
        /// the method returns a pair of null and an offset which is equal to the given
        /// <paramref name="offset"/>.
        /// </returns>
        /// <remarks>
        /// This method does not check if the authority component of the URI has only permitted
        /// characters. Though <see cref="GetUserInformationComponent"/>, <see cref="GetHostComponent"/> and
        /// <see cref="GetPortComponent"/> does so for their components.
        /// </remarks>
        internal static (AuthorityComponent Authority, int Offset) FromString(string uriString, int offset)
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
                    ? (new AuthorityComponent(uriString.Substring(offset + 2)), uriString.Length)
                    : (new AuthorityComponent(uriString[(offset + 2)..endOfAuthority]), endOfAuthority);
            }
            catch (ArgumentOutOfRangeException)
            {
                return (null, offset);
            }
        }

        /// <summary>
        /// This method converts the Authority component into a string and appends it into the
        /// given <paramref name="uriBuilder" /> <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="uriBuilder">
        /// This is the <see cref="StringBuilder" /> into which the Authority component will be added.
        /// </param>
        internal void BuildEncodedString(StringBuilder uriBuilder)
        {
            if (Authority != null)
            {
                uriBuilder.Append("//");
                _userInformation?.BuildEncodedString(uriBuilder);
                _host?.BuildEncodedString(uriBuilder);
                _port?.BuildEncodedString(uriBuilder);
            }
        }
    }
}
