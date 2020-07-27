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

                pathSegments = pathSegments.Select(segment => DecodeFromPercentEncoded(segment)).ToList();

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

                pathSegments = pathSegments.Select(segment => DecodeFromPercentEncoded(segment)).ToList();

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

            ':', '@',
        };

        /// <summary>
        /// This method converts the Path component into a string and appends it into the
        /// given <paramref name="uriBuilder" /> <see cref="System.Text.StringBuilder" />.
        /// </summary>
        /// <param name="uriBuilder">
        /// This is the <see cref="System.Text.StringBuilder" /> into which the Path
        /// component will be added.
        /// </param>
        private void PathToString(StringBuilder uriBuilder)
        {
            if (Path.Count == 0)
            {
                return;
            }

            if (Path[0] == string.Empty && Path.Count == 1)
            {
                uriBuilder.Append("/");
            }
            else
            {
                uriBuilder.AppendJoin('/', Path);
            }
        }

    }
}
