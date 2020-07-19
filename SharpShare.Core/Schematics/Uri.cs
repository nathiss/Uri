#region Copyrights
// This file is a part of the SharpShare.
//
// Copyright (c) 2020 Kamil Rusin
// Licensed under the MIT License.
// See LICENSE.txt file in the project root for full license information.
#endregion

using System;
using System.Collections.Generic;
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
                var scheme = ParseScheme(uriString);
                if (scheme.Scheme == null)
                {
                    return null;
                }

                var uri = new Uri {Scheme = scheme.Scheme};

                var authority = ParseAuthority(uriString, scheme.Offset);

                if (authority.Authority != null)
                {
                    var authorityBuilder = new StringBuilder();

                    var userInformation = ParseUserInformation(authority.Authority);
                    uri.UserInformation = userInformation.UserInformation;
                    if (uri.HasUserInformation)
                    {
                        authorityBuilder.Append(uri.UserInformation).Append('@');
                    }

                    var host = ParseHost(authority.Authority, userInformation.Offset);
                    uri.Host = host.Host;
                    authorityBuilder.Append(uri.Host);

                    uri.Port = ParsePort(authority.Authority, host.Offset);
                    if (uri.HasPort)
                    {
                        authorityBuilder.Append(':').Append(uri.Port);
                    }

                    uri.Authority = authorityBuilder.ToString();
                }

                var path = ParsePath(uriString, authority.Offset, uri.Authority != null);
                uri.Path = path.Path;

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
        private static (string Scheme, int Offset) ParseScheme(string uriString)
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
        private static (string Authority, int Offset) ParseAuthority(string uriString, int offset)
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
        private static (string UserInformation, int Offset) ParseUserInformation(string authority)
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
        /// a pair of null and an offset equal to the given <paramref name="offset"/>.
        /// </summary>
        /// <param name="authority">
        /// This is the authority component of the URI. If this parameter is equal to null,
        /// it means that the URI does not have the authority component.
        /// </param>
        /// <param name="offset">
        /// This is the offset of the given <paramref name="authority"/> after which the
        /// lookout will be performed. If the offset is greater or equal to the length
        /// of <paramref name="authority"/> and the URI has the authority component,
        /// this method will return a pair of null and an offset equal to the given
        /// offset.
        /// </param>
        /// <returns>
        /// A pair of a lowercase string representing the host component of the URI and the
        /// offset of the rest of the given <paramref name="authority"/> is returned.
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
        private static (string Host, int Offset) ParseHost(string authority, int offset)
        {
            if (string.IsNullOrWhiteSpace(authority))
            {
                return (null, offset);
            }

            var hostString = authority.Substring(offset).ToLower();

            if (!hostString.All(ch => HostAllowedCharacters.Contains(ch)))
            {
                throw new InvalidUriException();
            }

            var endOfHost = authority.IndexOf(':', offset);

            return endOfHost == -1 ?
                (hostString, authority.Length) :
                (authority.Substring(offset, endOfHost - offset), endOfHost + 1);
        }

        /// <summary>
        /// This collection contains all possible characters that can be used inside a
        /// Host component.
        /// </summary>
        private static HashSet<char> HostAllowedCharacters { get; } = new HashSet<char>
        {
            // sub-delims
            '!', '$' , '&' , '\'' , '(' , ')' , '*' , '+' , ',' , ';' , '=',

            // ASCII letters
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l','m',
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',

            // ASCII decimal digits
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',

            // rest of unreserved
            '-', '.', '_', '~',

            // colon
            ':',
        };

        /// <summary>
        /// This method parses the given <paramref name="authority"/> and returns the port of
        /// the URI, if the URI does not have the authority component this method will return
        ///  -1. If the Port component of the URI is empty, this method will return 0.
        /// </summary>
        /// <param name="authority">
        /// This is the authority component of the URI. This parameter must not be null.
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
        private static int ParsePort(string authority, int offset)
        {
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
        private static (IList<string> Path, int Offset) ParsePath(string uriString, int offset, bool hasAuthority)
        {
            if (offset >= uriString.Length)
            {
                return (new List<string>(), offset);
            }

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

                var endOfPath = uriString.IndexOfAny(new[] {'?', '#'}, offset);
                var pathString = endOfPath == -1
                    ? uriString.Substring(offset)
                    : uriString.Substring(offset, endOfPath - offset);

                if (pathString == "/")
                {
                    return (new List<string> {""}, endOfPath);
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

                return (pathSegments, endOfPath - offset);
            }

            // "Path component cannot begin with two slashes ("//")."
            // Scenario in which the path component begins with two slashes is not
            // possible since if the URI string contains two slashes after the scheme
            // component we're treating the following as an authority component.
            {
                var endOfPath = uriString.IndexOfAny(new[] {'?', '#'}, offset);
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

                return (pathSegments, endOfPath);
            }
        }

        /// <summary>
        /// This collection contains all possible characters that can be used inside a
        /// path component.
        /// </summary>
        private static readonly HashSet<char> PathAllowedCharacters = new HashSet<char>
        {
            '/',

            // PCHAR

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

            ':', '@'
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
