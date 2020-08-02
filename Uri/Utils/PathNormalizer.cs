#region Copyrights
// This file is a part of the Uri.
//
// Copyright (c) 2020 Kamil Rusin
// Licensed under the MIT License.
// See LICENSE.txt file in the project root for full license information.
#endregion

using System.Collections.Generic;
using System.Linq;

namespace Uri.Utils
{
    /// <summary>
    /// This class provides functionality to normalize Path component.
    /// </summary>
    internal static class PathNormalizer
    {
        /// <summary>
        /// This method normalizes the URI's path given as <paramref name="segments" />.
        /// </summary>
        /// <param name="segments">
        /// This is the collection of path segments of the URI.
        /// </param>
        /// <returns>
        /// A collection of normalized Path segments is returned.
        /// </returns>
        internal static IList<string> Normalize(IList<string> segments)
        {
            // If the path consists of one or less segments, then there's nothing to
            // normalize.
            if (segments.Count <= 1)
            {
                return segments;
            }

            var normalizedSegments = new List<string>();
            foreach (var segment in segments)
            {
                if (segment == ".")
                {
                    continue;
                }
                if (segment == "..")
                {
                    if (!normalizedSegments.Any())
                    {
                        normalizedSegments.Add(segment);
                        continue;
                    }

                    var lastElementIndex = normalizedSegments.Count - 1;
                    if (normalizedSegments[lastElementIndex] == "..")
                    {
                        normalizedSegments.Add(segment);
                        continue;
                    }

                    if (normalizedSegments[lastElementIndex] == string.Empty)
                    {
                        continue;
                    }

                    normalizedSegments.RemoveAt(lastElementIndex);
                    continue;
                }

                normalizedSegments.Add(segment);
            }
            return normalizedSegments;
        }
    }
}
