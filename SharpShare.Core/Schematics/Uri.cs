#region Copyrights
// This file is a part of the SharpShare.
//
// Copyright (c) 2020 Kamil Rusin
// Licensed under the MIT License.
// See LICENSE.txt file in the project root for full license information.
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SharpShare.Core.Utils;

namespace SharpShare.Core.Schematics
{
    /// <summary>
    /// This class represents a Universal Resource Identifier (URI) which is
    /// documented by <see href="https://tools.ietf.org/html/rfc3986">RFC 3986</see>.
    /// </summary>
    public class Uri
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
        /// This property represents the User Information of a URI. If the user information
        /// component was not present in the URI, the value of this property will be null.
        /// </summary>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.2.1">RFC 3986 (Section 3.2.1)</seealso>
        public string UserInformation { get; private set; }

        /// <summary>
        /// This property returns an indication of whether or not the URI has a UserInformation component.
        /// </summary>
        public bool HasUserInformation => UserInformation != null;

        /// <summary>
        /// This property represents the Host of a URI. If the host component was not present
        /// in the URI, the value of this property will be null.
        /// </summary>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.2.2">RFC 3986 (Section 3.2.2)</seealso>
        public string Host { get; private set; }

        /// <summary>
        /// This property returns an indication of whether or not the URI has a Host component.
        /// </summary>
        public bool HasHost => Host != null;

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
        /// This property represents the Path of a URI. If the path component of the URI
        /// was empty this property will contain a collection of zero elements.
        /// </summary>
        /// <remarks>
        /// If the path of the URI was an absolute path then the first element of this
        /// collection will be an empty string.
        /// </remarks>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.3">RFC 3986 (Section 3.3)</seealso>
        public IList<string> Path { get; private set; }

        /// <summary>
        /// This property returns an indication of whether or not the Path component of the URI is empty.
        /// </summary>
        public bool HasEmptyPath => Path.Count == 0;

        /// <summary>
        /// This property represents the Query component of a URI. If the component was not present
        /// in the URI, then the value of this property is null.
        /// </summary>
        /// <remarks>
        /// If the Query component is present but empty, the URI this property is
        /// <seealso cref="string.Empty"/>.
        /// </remarks>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.4">RFC 3986 (Section 3.4)</seealso>
        public string QueryString { get; private set; }

        /// <summary>
        /// This property returns an indication of whether or not the URI has the Query component.
        /// </summary>
        public bool HasQuery => QueryString != null;

        /// <summary>
        /// This property represents the Query component of the URI parsed into key-values pairs.
        /// </summary>
        /// <value>
        /// Inside a Query component two values can have the same key.
        /// If the URI doesn't have a Query component, the value of this property is null.
        /// If the URI have a Query component, but it's empty, the value of this property is a
        /// collection of zero elements.
        /// </value>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.4">RFC 3986 (Section 3.4)</seealso>
        public IReadOnlyList<KeyValuePair<string, string>> Query { get; private set; }

        /// <summary>
        /// This property represents the Fragment component of the URI. If the URI does not have the Fragment
        /// component, then the value of this property is null.
        /// </summary>
        /// <value>
        /// If the Fragment component is present but empty, the value of this property is
        /// <seealso cref="string.Empty"/>.
        /// </value>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.5">RFC 3986 (Section 3.5)</seealso>
        public string Fragment { get; private set; }

        /// <summary>
        /// This property returns an indication of whether or not the URI has the Fragment component.
        /// </summary>
        public bool HasFragment => Fragment != null;

        /// <summary>
        /// This is the default constructor of the <see cref="Uri"/> class.
        /// Since this constructor is private the only was of create a new <see cref="Uri"/>
        /// object is through <see cref="FromString"/> static method.
        /// </summary>
        private Uri() {}

        /// <summary>
        /// This method parses the given <paramref name="uriString"/> and returns
        /// the scheme of the URI and the offset of the rest of <paramref name="uriString"/>.
        /// </summary>
        /// <param name="uriString">
        /// This is the URI encoded as string. This parameter must not be null.
        /// </param>
        /// <returns>
        /// A pair of a lowercase string representing the scheme of the URI and the
        /// offset of the reset of <paramref name="uriString"/> is returned.
        /// if the URI does not contain the scheme, which is an equivalent of being
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
        /// This collection contains all possible characters that can be used inside a URI scheme.
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
                    : (uriString.Substring(offset + 2, endOfAuthority - (offset + 2)), endOfAuthority);
            }
            catch (ArgumentOutOfRangeException)
            {
                return (null, offset);
            }
        }

        /// <summary>
        /// This method parses the given <paramref name="authority"/> and returns the user information
        /// of the URI, if the URI does not have a user information this method will return a pair
        /// of null and zero.
        /// </summary>
        /// <param name="authority">
        /// This is the authority component of the URI.
        /// </param>
        /// <returns>
        /// A pair of a string representing the user information and the offset of the rest
        /// of <paramref name="authority"/> is returned. If the authority does not have a user
        /// information component this method returns a pair of null and zero.
        /// If the given <paramref name="authority"/> string is null, this method returns a pair of
        /// null and zero.
        /// </returns>
        private static (string UserInformation, int Offset) GetUserInformationComponent(string authority)
        {
            if (string.IsNullOrWhiteSpace(authority))
            {
                return (null, 0);
            }

            var endOfUserInformation = authority.IndexOf('@');
            if (endOfUserInformation == -1)
            {
                return (null, 0);
            }

            var userInformation = authority.Substring(0, endOfUserInformation);

            if (!userInformation.All(ch => UserInformationAllowedCharacters.Contains(ch)))
            {
                throw new InvalidUriException();
            }

            return (userInformation, endOfUserInformation + 1);
        }

        /// <summary>
        /// This collection contains all possible characters that can be used inside a user information component.
        /// </summary>
        private static readonly HashSet<char> UserInformationAllowedCharacters = new HashSet<char>
        {
            // Unreserved characters

            // Alpha lowercase
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l','m',
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

            '!', '$' , '&' , '\'' , '(' , ')' , '*' , '+' , ',' , ';' , '=',

            ':',
        };

        /// <summary>
        /// This method parses the given <paramref name="authority"/> and returns the host of
        /// the URI, if the URI does not have the authority component this method will return
        /// a tuple of null, an offset equal to the given <paramref name="offset"/> and false.
        /// </summary>
        /// <param name="authority">
        /// This is the authority component of the URI. This parameter must not be empty.
        /// </param>
        /// <param name="offset">
        /// This is the offset of the given <paramref name="authority"/> after which the
        /// lookout will be performed. If the offset is greater or equal to the length
        /// of <paramref name="authority"/> and the URI has the authority component,
        /// this method will return a tuple of null, an offset equal to the given
        /// offset and false.
        /// </param>
        /// <returns>
        /// A tuple of a lowercase string representing the host component of the URI, the
        /// offset of the rest of the given <paramref name="authority"/> and an indication
        /// of whether or not the host component is a ip-literal is returned.
        /// If the URI has the authority component, but does not have the host component,
        /// then the URI is ill-formed and this method will throw an exception of type
        /// <see cref="InvalidUriException"/>.
        /// </returns>
        /// <exception cref="InvalidUriException">
        /// An exception of this type is thrown if
        /// <list type="bullet">
        /// <item>
        /// the URI has the authority component, but does not have the host component;
        /// </item>
        /// <item>
        /// the user information component contains unallowed characters.
        /// </item>
        /// </list>
        /// </exception>
        private static (string Host, int Offset, bool IsIpLiteral) GetHostComponent(string authority, int offset)
        {
            // If the authority component is present, then the host component is mandatory, though
            // it might be empty.
            if (authority == string.Empty)
            {
                return (string.Empty, offset, false);
            }

            // Parse IP-Literal
            if (authority[offset] == '[')
            {
                var endOfIpLiteralHost = authority.IndexOf(']', offset);
                if (endOfIpLiteralHost == -1)
                {
                    throw new InvalidUriException();
                }

                var colonAfterIpLiteralHost = authority.IndexOf(':', endOfIpLiteralHost);
                var ipLiteralString = authority.Substring(offset + 1, endOfIpLiteralHost - (offset + 1));

                // IPv6
                var ip6Address = ParseIp6Address(ipLiteralString);
                if (ip6Address != null)
                {
                    return colonAfterIpLiteralHost == -1
                        ? (ip6Address, endOfIpLiteralHost + 1, true)
                        : (ip6Address, endOfIpLiteralHost + 2, true);
                }

                // IPvFuture
                var ipFuture = ParseIpFuture(ipLiteralString);
                if (ipFuture != null)
                {
                    return colonAfterIpLiteralHost == -1
                        ? (ipFuture, endOfIpLiteralHost + 1, true)
                        : (ipFuture, endOfIpLiteralHost + 2, true);
                }

                throw new InvalidUriException();
            }

            var endOfHost = authority.IndexOf(':', offset);

            var hostString = endOfHost == -1
                ? authority.Substring(offset)
                : authority.Substring(offset, endOfHost - offset);

            if (!hostString.All(ch => HostAllowedCharacters.Contains(ch)))
            {
                throw new InvalidUriException();
            }

            // Since IPv4 address is a valid reg-name is terms of syntax, we only
            // try to parse reg-name.
            var regNameHost = ParseRegNameHost(hostString);
            if (regNameHost != null)
            {
                return endOfHost == -1 ?
                    (regNameHost, authority.Length, false) :
                    (regNameHost, endOfHost + 1, false);
            }

            throw new InvalidUriException();
        }

        /// <summary>
        /// This collection contains all possible characters that can be used inside a
        /// Host component.
        /// </summary>
        private static HashSet<char> HostAllowedCharacters { get; } = new HashSet<char>
        {
            // Unreserved

            // Alpha lowercase
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l','m',
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

            '!', '$' , '&' , '\'' , '(' , ')' , '*' , '+' , ',' , ';' , '=',

            // Hex digit
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'A', 'B', 'C', 'D', 'E', 'F',

             ':',
        };

        /// <summary>
        /// This method parses the given <paramref name="ipLiteral"/> and returns a string
        /// representing IPv6 address.
        /// </summary>
        /// <param name="ipLiteral">
        /// This is the string representing IP Literal. This parameter must not be null.
        /// </param>
        /// <returns>
        /// A string representing IPv6 is returned. If the given <paramref name="ipLiteral"/>
        /// is not a IPv6 address this method return null.
        /// </returns>
        private static string ParseIp6Address(string ipLiteral)
        {
            ipLiteral = ipLiteral.ToLower();
            if (!ipLiteral.All(ch => AllowedIpv6Characters.Contains(ch)))
            {
                return null;
            }

            var segments = ipLiteral.Split(':');
            if (segments.Length == 0 || segments.Length > 8)
            {
                return null;
            }

            // If a colon is the first character in a IPv6 address, the second
            // one also must be a colon.
            if (segments[0] == string.Empty && segments[1] != string.Empty)
            {
                return null;
            }

            // If a colon is the last character in a IPv6 address, the second
            // from the end also must be a colon.
            if (segments[^1] == string.Empty && segments[^2] != string.Empty)
            {
                return null;
            }

            try
            {
                // In a IPv6 address there can be only one double colon ("::")
                // and any other colon sequences are not allowed.
                if (segments.Count(seg => seg == string.Empty) >= 2)
                {
                    if (!(ipLiteral.Substring(0, 2) == "::" ||
                          ipLiteral.Substring(ipLiteral.Length - 2, 2) == "::"))
                    {
                        return null;
                    }
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }


            if (!segments.All(IsValidIpv6Segment))
            {
                return null;
            }

            return string.Join(':', segments.Select(seg =>
            {
                if (seg == string.Empty)
                {
                    return string.Empty;
                }
                var trimmed = seg.TrimStart('0');
                return trimmed == string.Empty ? "0" : trimmed;
            }));
        }

        /// <summary>
        /// This method returns an indication of whether or not the given
        /// <paramref name="segment"/> is a valid IPv6 segment.
        /// </summary>
        /// <param name="segment">
        /// This is a IPv6 segment. This parameter must not be null.
        /// </param>
        /// <returns>
        /// A indication of of whether or not the given <paramref name="segment"/>
        /// is a valid IPv6 segment is returned.
        /// </returns>
        private static bool IsValidIpv6Segment(string segment)
        {
            return segment.Length <= 4 &&
                   segment.All(ch => AllowedIpv6SegmentCharacters.Contains(ch));
        }

        /// <summary>
        /// This collection contains all possible characters that can bu used inside a
        /// IPv6 address.
        /// </summary>
        private static readonly HashSet<char> AllowedIpv6Characters = new HashSet<char>
        {
            // Hex digits
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'a', 'b', 'c', 'd', 'e', 'f',

            // Colon
            ':',
        };

        /// <summary>
        /// This collection contains all possible characters that can bu used inside a
        /// IPv6 address segment.
        /// </summary>
        private static readonly HashSet<char> AllowedIpv6SegmentCharacters = new HashSet<char>
        {
            // Hex digits
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'a', 'b', 'c', 'd', 'e', 'f',
        };

        /// <summary>
        /// This method parses the given <paramref name="ipLiteral"/> and returns a string
        /// representing IPvFuture address.
        /// </summary>
        /// <param name="ipLiteral">
        /// This is the string representing IP Literal. This parameter must not be null.
        /// </param>
        /// <returns>
        /// A string representing IPvFuture is returned. If the given <paramref name="ipLiteral"/>
        /// is not a IPvFuture address this method return null.
        /// </returns>
        private static string ParseIpFuture(string ipLiteral)
        {
            ipLiteral = ipLiteral.ToLower();
            if (ipLiteral[0] != 'v')
            {
                return null;
            }

            var firstDot = ipLiteral.IndexOf('.');
            if (firstDot == -1)
            {
                return null;
            }

            // First segment of a IPvFuture address must be a hex number.
            var firstHexNumber = ipLiteral.Substring(1, firstDot - 1);
            if (!int.TryParse(firstHexNumber, NumberStyles.HexNumber, null, out _))
            {
                return null;
            }

            if (!ipLiteral.All(ch => AllowedIpFutureCharacters.Contains(ch)))
            {
                throw new InvalidUriException();
            }

            // After the first dot there must be at least one more valid character.
            if (ipLiteral.Length <= firstDot + 2)
            {
                return null;
            }

            return ipLiteral;
        }

        private static readonly HashSet<char> AllowedIpFutureCharacters = new HashSet<char>
        {
            // Unreserved

            // Alpha lowercase
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l','m',
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',

            // Alpha uppercase
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',

            // Digit
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',

            '-', '.', '_', '~',

            // sub-delims

            '!', '$' , '&' , '\'' , '(' , ')' , '*' , '+' , ',' , ';' , '=',

            ':',
        };

        /// <summary>
        /// This method parses the given <paramref name="hostString"/> and returns a string
        /// representing registered name.
        /// </summary>
        /// <param name="hostString">
        /// This is the string representing host component. This parameter must not be null.
        /// </param>
        /// <returns>
        /// A string representing registered name is returned. If the given <paramref name="hostString"/>
        /// is not a registered name this method return null.
        /// </returns>
        private static string ParseRegNameHost(string hostString)
        {
            return hostString.All(ch => HostAllowedCharacters.Contains(ch)) ?
                hostString.ToLower() :
                null;
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

        /// <summary>
        /// This method parses the given <paramref name="uriString"/> and returns the path
        /// of the URI.
        /// </summary>
        /// <param name="uriString">
        /// This is the URI encoded as string. This parameter must not be null.
        /// </param>
        /// <param name="offset">
        /// This is the offset of the given <paramref name="uriString"/> after which
        /// the lookout will be performed. If the offset is greater or equal to the
        /// length of <paramref name="uriString"/>, this method will return a pair of
        /// empty collection and the given offset.
        /// </param>
        /// <param name="hasAuthority">
        /// An indication of whether of not the URI has an authority component.
        /// </param>
        /// <returns>
        /// A pair of collection of path segments and the offset of the rest of
        /// <paramref name="uriString"/> is returned. If the path segment of the URI
        /// is empty this method will return a pair of empty collection and an offset
        /// equal to the given <paramref name="offset"/>.
        /// </returns>
        /// <exception cref="InvalidUriException">
        /// An exception of this type is thrown if:
        /// <list type="bullet">
        /// <item>
        /// the URI has the authority component, but the path do not begin with a slash ("/");
        /// </item>
        /// <item>
        /// the URI does not have the authority component and the first segment of the path
        /// is empty;
        /// </item>
        /// <item>
        /// the URI does not have the authority component and the first segment of the path
        /// contains a colon (":").
        /// </item>
        /// </list>
        /// </exception>
        private static (IList<string> Path, int Offset) GetPathComponent(string uriString, int offset, bool hasAuthority)
        {
            if (offset >= uriString.Length)
            {
                return (new List<string>(), offset);
            }

            var endOfPath = uriString.IndexOfAny(new[] {'?', '#'}, offset);
            var newOffset = endOfPath == -1 ? uriString.Length : endOfPath;

            if (hasAuthority)
            {
                // Path component must either be empty of begin with a slash ("/").
                if (uriString[offset] == '?' || uriString[offset] == '#')
                {
                    return (new List<string>() , offset);
                }

                if (uriString[offset] != '/')
                {
                    throw new InvalidUriException();
                }

                var pathString = endOfPath == -1
                    ? uriString.Substring(offset)
                    : uriString.Substring(offset, endOfPath - offset);

                if (pathString == "/")
                {
                    return (new List<string> {""}, newOffset);
                }

                if (!pathString.All(ch => PathAllowedCharacters.Contains(ch)))
                {
                    throw new InvalidUriException();
                }

                var pathSegments = pathString.Split('/').ToList();

                if (pathSegments.Count >= 2)
                {
                    // If the path is absolute, the first segment must not be empty.
                    // path-absolute = "/" [ segment-nz *( "/" segment ) ]
                    if (pathSegments[0] == string.Empty && pathSegments[1] == string.Empty)
                    {
                        throw new InvalidUriException();
                    }
                }

                return (pathSegments, newOffset);
            }

            // "Path component cannot begin with two slashes ("//")."
            // Scenario in which the path component begins with two slashes is not
            // possible since if the URI string contains two slashes after the scheme
            // component we're treating the following as an authority component.
            {
                var pathString = endOfPath == -1
                    ? uriString.Substring(offset)
                    : uriString.Substring(offset, endOfPath - offset);

                if (!pathString.All(ch => PathAllowedCharacters.Contains(ch)))
                {
                    throw new InvalidUriException();
                }

                var pathSegments = pathString.Split('/').ToList();
                if (pathSegments[0].Contains(':'))
                {
                    // If the URI does not contain an authority component, the first
                    // segment of the path component cannot contains a colon (":").
                    throw new InvalidUriException();
                }

                return (pathSegments, newOffset);
            }
        }

        /// <summary>
        /// This collection contains all possible characters that can be used inside a
        /// Path component.
        /// </summary>
        private static readonly HashSet<char> PathAllowedCharacters = new HashSet<char>
        {
            '/',

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

            ':', '@'
        };

        /// <summary>
        /// This method parses the given <paramref name="uriString"/> and returns the query of
        /// the URI, if the URI does not have the query component this method will return
        ///  a pair of null and an offset equal to the given <paramref name="offset"/>.
        /// If the Query component of the URI is empty, this method will return a pair of
        /// <see cref="string.Empty"/> and an offset equal to the given <paramref name="offset"/>.
        /// </summary>
        /// <param name="uriString">
        /// This is the string representation of the URI. This parameter must not be null.
        /// </param>
        /// <param name="offset">
        /// This is the offset of the given <paramref name="uriString"/> after which the
        /// lookout will be performed. If the offset is greater or equal to the length
        /// of <paramref name="uriString"/> this method will return a pair of null and an
        /// offset equal to the given <paramref name="offset"/>.
        /// </param>
        /// <returns>
        /// A pair of the string representation of the Query component and an offset
        /// of the rest of the <paramref name="uriString"/> is returned.
        /// </returns>
        /// <exception cref="InvalidUriException">
        /// If the query component contains not allowed characters an exception of this type
        /// is thrown.
        /// </exception>
        private static (string Query, int Offset) GetQueryComponent(string uriString, int offset)
        {
            if (offset >= uriString.Length || uriString[offset] != '?')
            {
                return (null, offset);
            }

            var endOfQuery = uriString.IndexOf('#', offset + 1);

            var query = endOfQuery == -1
                ? (uriString.Substring(offset + 1), uriString.Length)
                : (uriString.Substring(offset + 1, endOfQuery - offset - 1), endOfQuery);

            if (!query.Item1.All(ch => QueryAllowedCharacters.Contains(ch)))
            {
                throw new InvalidUriException();
            }

            return query;
        }

        /// <summary>
        /// This collection contains all possible characters that can be used inside a Query
        /// component.
        /// </summary>
        private static readonly HashSet<char> QueryAllowedCharacters = new HashSet<char>
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
        /// This method parses the given <paramref name="query"/> component and return a collection
        /// of key-value pairs.
        /// </summary>
        /// <param name="query">
        /// This is the query component of the URI. The Query component must be well-formed.
        /// </param>
        /// <returns>
        /// A collection of key-value pairs representing the URI's query component is returned.
        /// If the URI doesn't have the Query component this method returns null.
        /// If the Query component of the URI is empty, this method returns a collection of zero
        /// elements.
        /// </returns>
        /// <remarks>
        /// If a key-value element of the Query component has more than one equal sign ("="), then
        /// the first occurence of the equal sign will be treated as a separator and the others as
        /// a part of value.
        /// </remarks>
        private static IReadOnlyList<KeyValuePair<string, string>> ParseQuery(string query)
        {
            if (query == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<KeyValuePair<string, string>>();
            }

            return query
                .Split('&')
                .Select(keyValueString => keyValueString.Split('=', 2))
                .Select(
                    keyValuePair => keyValuePair.Length == 1
                        ? new KeyValuePair<string, string>(keyValuePair[0], null)
                        : new KeyValuePair<string, string>(keyValuePair[0], keyValuePair[1])
                )
                .ToList();
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
        /// A string representation of the Fragment component is returned.
        /// </returns>
        /// <exception cref="InvalidUriException">
        /// If the Fragment component contains not allowed characters an exception of this type
        /// is thrown.
        /// </exception>
        private static string GetFragmentComponent(string uriString, int offset)
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

            return targetString;
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
