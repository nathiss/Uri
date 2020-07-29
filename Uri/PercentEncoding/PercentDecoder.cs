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
using Uri.Exceptions;

namespace Uri.PercentEncoding
{
    /// <summary>
    /// This class provider the functionality to decode percent-encoded characters.
    /// It's used by <see cref="Uri" /> class when decomposing URIs.
    /// </summary>
    public static class PercentDecoder
    {
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
        public static string Decode(string percentEncodedSegment)
        {
            try
            {
                var decodedStringBuilder = new StringBuilder();

                var nextIndexToAnalyse = 0;
                var indexOfPercentChar = percentEncodedSegment.IndexOf('%');
                while (indexOfPercentChar != -1)
                {
                    decodedStringBuilder.Append(percentEncodedSegment[nextIndexToAnalyse..indexOfPercentChar]);

                    var hexValueString = percentEncodedSegment.Substring(indexOfPercentChar + 1, 2);

                    if (!int.TryParse(hexValueString, NumberStyles.HexNumber, null, out var hexValue))
                    {
                        throw new InvalidUriException();
                    }

                    decodedStringBuilder.Append(Convert.ToChar(hexValue));

                    nextIndexToAnalyse = indexOfPercentChar + 3;
                    indexOfPercentChar = percentEncodedSegment.IndexOf('%', nextIndexToAnalyse);
                }

                decodedStringBuilder.Append(percentEncodedSegment.Substring(nextIndexToAnalyse));

                return decodedStringBuilder.ToString();
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new InvalidUriException();
            }
        }
    }
}
