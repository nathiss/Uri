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
using Uri.Exceptions;
using Uri.PercentEncoding;

namespace Uri
{
    public partial class Uri
    {
        /// <summary>
        /// This property represents the Query component of a URI. If the component was not present
        /// in the URI, then the value of this property is null.
        /// </summary>
        /// <remarks>
        /// If the Query component is present but empty, the URI this property is
        /// <seealso cref="string.Empty"/>. This component's percent-encoded characters are not decoded.
        /// In order to use Query component with decoded percent-encoded characters use <see cref="Query" />.
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
        /// the first occurrence of the equal sign will be treated as a separator and the others as
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
                        ? new KeyValuePair<string, string>(PercentDecoder.Decode(keyValuePair[0]), null)
                        : new KeyValuePair<string, string>(
                            PercentDecoder.Decode(keyValuePair[0]),
                            PercentDecoder.Decode(keyValuePair[1])
                        )
                )
                .ToList();
        }

                /// <summary>
        /// This method converts the Query component into a string and appends it into the
        /// given <paramref name="uriBuilder" /> <see cref="System.Text.StringBuilder" />.
        /// </summary>
        /// <param name="uriBuilder">
        /// This is the <see cref="System.Text.StringBuilder" /> into which the Query
        /// component will be added.
        /// </param>
        private void QueryToString(StringBuilder uriBuilder)
        {
            if (Query == null || Query.Count == 0)
            {
                return;
            }

            var keyValuePairs = Query
                .Select(kv =>
                {
                    if (kv.Value != null)
                    {
                        return $"{kv.Key}={kv.Value}";
                    }
                    return $"{kv.Key}";
                }
            );

            uriBuilder.Append('?').AppendJoin('&', keyValuePairs);
        }
    }
}
