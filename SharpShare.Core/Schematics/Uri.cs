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
            if (string.IsNullOrWhiteSpace(uriString))
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
                uri.Authority = authority.Authority;

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
        /// This property represents the Authority of a URI. If the authority component was
        /// not present if the URI, the value of this property will be null.
        /// </summary>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.2">RFC 3986 (Section 3.2)</seealso>
        public string Authority { get; private set; }

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
        /// This list contains all possible characters that can be used inside a URI scheme.
        /// </summary>
        private static IList<char> SchemeAllowedCharacters { get; } = new List<char>
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
        /// authority of the URI, if the URI has an authority.
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

                return pathString == "/" ?
                    (new List<string>{""}, endOfPath) :
                    (pathString.Split('/').ToList(), endOfPath);
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
