#region Copyrights
// This file is a part of the Uri.
//
// Copyright (c) 2020 Kamil Rusin
// Licensed under the MIT License.
// See LICENSE.txt file in the project root for full license information.
#endregion

using System;
using System.Text;
using Uri.Utils;

namespace Uri
{
    /// <summary>
    /// This class represents a Universal Resource Identifier (URI) which is
    /// documented by <see href="https://tools.ietf.org/html/rfc3986">RFC 3986</see>.
    /// </summary>
    public partial class Uri
    {

        /// <summary>
        /// This method builds a <seealso cref="Uri"/> object from the
        /// given <paramref name="uriString"/>.
        /// </summary>
        /// <param name="uriString">
        /// This is the URI string which will be used to build a <see cref="Uri"/>
        /// object.
        /// </param>
        /// <returns>
        /// If the operation was successful a <see cref="Uri" /> object is returned.
        /// Otherwise if the <paramref name="uriString"/> was ill-formed this method
        /// will return null.
        /// </returns>
        public static Uri FromString(string uriString)
        {
            if (string.IsNullOrWhiteSpace(uriString) || uriString.ContainsNonAsciiLetters())
            {
                return null;
            }

            try
            {
                var scheme = GetSchemeComponent(uriString);
                if (scheme.Scheme == null)
                {
                    return null;
                }

                var uri = new Uri {Scheme = scheme.Scheme};

                var authority = GetAuthorityComponent(uriString, scheme.Offset);

                if (authority.Authority != null)
                {
                    var authorityBuilder = new StringBuilder();

                    var userInformation = GetUserInformationComponent(authority.Authority);
                    uri.UserInformation = userInformation.UserInformation;
                    if (uri.HasUserInformation)
                    {
                        authorityBuilder.Append(uri.UserInformation).Append('@');
                    }

                    var host = GetHostComponent(authority.Authority, userInformation.Offset);
                    uri.Host = host.Host;
                    if (host.IsIpLiteral)
                    {
                        authorityBuilder.Append('[').Append(uri.Host).Append(']');
                    }
                    else
                    {
                        authorityBuilder.Append(uri.Host);
                    }

                    uri.Port = GetPortComponent(authority.Authority, host.Offset);
                    if (uri.HasPort)
                    {
                        authorityBuilder.Append(':').Append(uri.Port);
                    }

                    uri.Authority = authorityBuilder.ToString();
                }

                var path = GetPathComponent(uriString, authority.Offset, uri.Authority != null);
                uri.Path = path.Path;

                var query = GetQueryComponent(uriString, path.Offset);
                uri.QueryString = query.Query;
                uri.Query = ParseQuery(uri.QueryString);

                uri.Fragment = GetFragmentComponent(uriString, query.Offset);

                return uri;
            }
            catch (InvalidUriException)
            {
                return null;
            }
        }

        /// <summary>
        /// This is the default constructor of the <see cref="Uri"/> class.
        /// Since this constructor is private the only was of create a new <see cref="Uri"/>
        /// object is through <see cref="FromString"/> static method.
        /// </summary>
        private Uri() {}
    }


    /// <summary>
    /// This class represents an exception internally thrown by <see cref="Uri"/> parser when
    /// the URI string is ill-formed.
    /// </summary>
    [Serializable]
    internal sealed class InvalidUriException : Exception
    {
        public InvalidUriException() {}

        public InvalidUriException(string message)
            : base(message) {}
    }
}
