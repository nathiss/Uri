#region Copyrights
// This file is a part of the Uri.
//
// Copyright (c) 2020 Kamil Rusin
// Licensed under the MIT License.
// See LICENSE.txt file in the project root for full license information.
#endregion

using System;
using System.Globalization;
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
        /// This method returns a string representation of the object.
        /// </summary>
        /// <returns>
        /// A string representation of the object is returned.
        /// </returns>
        public override string ToString()
        {
            var uriBuilder = new StringBuilder();

            SchemeToString(uriBuilder);
            AuthorityToString(uriBuilder);
            PathToString(uriBuilder);
            QueryToString(uriBuilder);
            FragmentToString(uriBuilder);

            return uriBuilder.ToString();
        }

        /// <summary>
        /// This method returns a string with is a decoded version of the given
        /// <paramref name="encodedElement" />. If the <paramref name="encodedElement" />
        /// does not contain percent-encoded characters the returned string is equivalent to the
        /// given one.
        /// </summary>
        /// <param name="encodedElement">
        /// This is the percent-encoded version of the string.
        /// </param>
        /// <returns>
        ///  A string with is a decoded version of the given <paramref name="encodedElement" />
        /// is returned.
        /// </returns>
        /// <exception cref="InvalidUriException">
        /// An exception of this type is thrown if the given <paramref name="encodedElement" />
        /// contains a percent-encoded character which is invalid (e.g. two chars after "%" are not
        /// hexdigits).
        /// </exception>
        private static string DecodeFromPercentEncoded(string encodedElement)
        {
            try
            {
                var decodedStringBuilder = new StringBuilder();

                var nextIndexToAnalyse = 0;
                var indexOfPercentChar = encodedElement.IndexOf('%');
                while (indexOfPercentChar != -1)
                {
                    decodedStringBuilder.Append(encodedElement.Substring(nextIndexToAnalyse, indexOfPercentChar));

                    var hexValueString = encodedElement.Substring(indexOfPercentChar + 1, 2);

                    if (!int.TryParse(hexValueString, NumberStyles.HexNumber, null, out var hexValue))
                    {
                        throw new InvalidUriException();
                    }

                    decodedStringBuilder.Append(Convert.ToChar(hexValue));

                    nextIndexToAnalyse = indexOfPercentChar + 3;
                    indexOfPercentChar = encodedElement.IndexOf('%', nextIndexToAnalyse);
                }

                decodedStringBuilder.Append(encodedElement.Substring(nextIndexToAnalyse));

                return decodedStringBuilder.ToString();
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new InvalidUriException();
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
