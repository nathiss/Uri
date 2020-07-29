#region Copyrights
// This file is a part of the Uri.
//
// Copyright (c) 2020 Kamil Rusin
// Licensed under the MIT License.
// See LICENSE.txt file in the project root for full license information.
#endregion

using System;

namespace Uri.Exceptions
{
    /// <summary>
    /// This class represents an exception internally thrown by parsering component
    /// of this package when the URI string is ill-formed.
    /// </summary>
    [Serializable]
    public sealed class InvalidUriException : Exception
    {
        /// <summary>
        /// This is the default constructor of this class.
        /// </summary>
        public InvalidUriException() {}

        /// <summary>
        /// This constructor takes the given <paramref name="message" /> and passes it
        /// to the parent constructor.
        /// </summary>
        /// <param name="message">
        /// This is the exception message passed to the parent constructor.
        /// </param>
        public InvalidUriException(string message)
            : base(message) {}
    }
}
