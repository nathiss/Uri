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
    /// <summary>
    /// This class represents the UserInformation component of a URI.
    /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.2.1">RFC 3986 (Section 3.2.1)</seealso>
    /// </summary>
    internal class UserInformationComponent
    {
        /// <summary>
        /// This property represents the User Information of a URI. If the user information
        /// component was not present in the URI, the value of this property will be null.
        /// </summary>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.2.1">RFC 3986 (Section 3.2.1)</seealso>
        internal string UserInformation { get; }

        /// <summary>
        /// This constructor sets the value of <see cref="UserInformation" /> to the given
        /// <paramref name="userInformationString" />.
        /// </summary>
        /// <param name="userInformationString">
        /// This is a string representation of the UserInformation component of the URI.
        /// </param>
        private UserInformationComponent(string userInformationString)
        {
            UserInformation = userInformationString;
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
        /// A pair of an <see cref="UserInformation" /> object and the offset of the rest
        /// of <paramref name="authority"/> is returned. If the authority does not have a user
        /// information component this method returns a pair of null and zero.
        /// If the given <paramref name="authority"/> string is null, this method returns a pair of
        /// null and zero.
        /// </returns>
        internal static (UserInformationComponent UserInformation, int Offset) FromString(string authority)
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

            var userInformationString = authority.Substring(0, endOfUserInformation);

            if (!userInformationString.All(ch => UserInformationAllowedCharacters.Contains(ch)))
            {
                throw new InvalidUriException();
            }

            var userInformation = new UserInformationComponent(PercentDecoder.Decode(userInformationString));

            return (userInformation, endOfUserInformation + 1);
        }

        /// <summary>
        /// This collection contains all possible characters that can be used inside a UserInformation
        /// component.
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
        /// This method converts the UserInformation component into a string and appends it into the
        /// given <paramref name="uriBuilder" /> <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="uriBuilder">
        /// This is the <see cref="StringBuilder" /> into which the UserInformation component will be added.
        /// </param>
        internal void BuildEncodedString(StringBuilder uriBuilder)
        {
            if (UserInformation != null)
            {
                uriBuilder
                    .Append(PercentEncoder.Encode(UserInformation, UserInformationAllowedCharacters))
                    .Append('@');
            }
        }
    }
}
