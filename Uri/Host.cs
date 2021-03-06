﻿#region Copyrights
// This file is a part of the Uri.
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
using Uri.Exceptions;
using Uri.PercentEncoding;

namespace Uri
{
    /// <summary>
    /// This class represents the Host component of a URI.
    /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.2.2">RFC 3986 (Section 3.2.2)</seealso>
    /// </summary>
    internal class HostComponent
    {
        /// <summary>
        /// This property represents the Host of a URI. If the host component was not present
        /// in the URI, the value of this property will be null.
        /// </summary>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.2.2">RFC 3986 (Section 3.2.2)</seealso>
        internal string Host { get; }

        /// <summary>
        /// This property holds an indication of whether or not the Host component is a IP Literal Host.
        /// </summary>
        internal bool IsIpLiteralHost { get; }

        /// <summary>
        /// This constructor sets the value of <see cref="Host" /> to the given <paramref name="hostString" />.
        /// </summary>
        /// <param name="hostString">
        /// This is a string representation of the Host component of the URI.
        /// </param>
        /// <param name="isIpLiteralHost">
        /// This is an indication of whether or not the Host component is a IP Literal Host.
        /// </param>
        private HostComponent(string hostString, bool isIpLiteralHost = false)
        {
            Host = hostString;
            IsIpLiteralHost = isIpLiteralHost;
        }

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
        /// A tuple of a <see cref="Host" /> object, the offset of the rest of the given
        /// <paramref name="authority"/> and an indication of whether or not the host
        /// component is a ip-literal is returned.
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
        internal static (HostComponent Host, int Offset, bool IsIpLiteral) FromString(string authority, int offset)
        {
            // If the authority component is present, then the host component is mandatory, though
            // it might be empty.
            if (authority == string.Empty)
            {
                return (new HostComponent(string.Empty), offset, false);
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
                var ipLiteralString = authority[(offset + 1)..endOfIpLiteralHost];

                // IPv6
                var ip6Address = ParseIp6Address(ipLiteralString);
                if (ip6Address != null)
                {
                    return colonAfterIpLiteralHost == -1
                        ? (new HostComponent(ip6Address, true), endOfIpLiteralHost + 1, true)
                        : (new HostComponent(ip6Address, true), endOfIpLiteralHost + 2, true);
                }

                // IPvFuture
                var ipFuture = ParseIpFuture(ipLiteralString);
                if (ipFuture != null)
                {
                    return colonAfterIpLiteralHost == -1
                        ? (new HostComponent(ipFuture, true), endOfIpLiteralHost + 1, true)
                        : (new HostComponent(ipFuture, true), endOfIpLiteralHost + 2, true);
                }

                throw new InvalidUriException();
            }

            var endOfHost = authority.IndexOf(':', offset);

            var hostString = endOfHost == -1
                ? authority.Substring(offset)
                : authority[offset..endOfHost];

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
                    (new HostComponent(regNameHost), authority.Length, false) :
                    (new HostComponent(regNameHost), endOfHost + 1, false);
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
            var firstHexNumber = ipLiteral[1..firstDot];
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

        /// <summary>
        /// This collection contains all possible characters that can be used inside a
        /// IPvFuture address.
        /// </summary>
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
        /// This method converts the Host component into a string and appends it into the
        /// given <paramref name="uriBuilder" /> <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="uriBuilder">
        /// This is the <see cref="StringBuilder" /> into which the Host component will be added.
        /// </param>
        internal void BuildEncodedString(StringBuilder uriBuilder)
        {
            if (Host != null)
            {
                if (IsIpLiteralHost)
                {
                    uriBuilder
                        .Append('[')
                        .Append(PercentEncoder.Encode(Host, ExtraAllowedIpLiteralHostCharacters))
                        .Append(']');
                }
                else
                {
                    uriBuilder.Append(PercentEncoder.Encode(Host));
                }

            }
        }

        /// <summary>
        /// This collection contains all extra characters that are allowed inside a IP Literal Host.
        /// </summary>
        private static readonly HashSet<char> ExtraAllowedIpLiteralHostCharacters = new HashSet<char>
        {
            ':',
        };
    }
}
