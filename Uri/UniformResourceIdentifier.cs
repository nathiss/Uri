#region Copyrights
// This file is a part of the Uri.
//
// Copyright (c) 2020 Kamil Rusin
// Licensed under the MIT License.
// See LICENSE.txt file in the project root for full license information.
#endregion

using System.Text;
using Uri.Utils;
using Uri.Exceptions;
using System.Collections.Generic;

namespace Uri
{
    /// <summary>
    /// This class represents a Universal Resource Identifier (URI) which is
    /// documented by <see href="https://tools.ietf.org/html/rfc3986">RFC 3986</see>.
    /// </summary>
    public class UniformResourceIdentifier
    {

        /// <summary>
        /// This method builds a <seealso cref="UniformResourceIdentifier"/> object from the
        /// given <paramref name="uriString"/>.
        /// </summary>
        /// <param name="uriString">
        /// This is the URI string which will be used to build a <see cref="UniformResourceIdentifier"/>
        /// object.
        /// </param>
        /// <returns>
        /// If the operation was successful a <see cref="UniformResourceIdentifier" /> object is returned.
        /// Otherwise if the <paramref name="uriString"/> was ill-formed this method
        /// will return null.
        /// </returns>
        public static UniformResourceIdentifier FromString(string uriString)
        {
            if (string.IsNullOrWhiteSpace(uriString) || uriString.ContainsNonAsciiLetters())
            {
                return null;
            }

            try
            {
                var scheme = SchemeComponent.FromString(uriString);
                var authority = AuthorityComponent.FromString(uriString, scheme.Offset);
                var path = PathComponent.FromString(uriString, authority.Offset, authority.Authority != null);
                var query = QueryComponent.FromString(uriString, path.Offset);
                var fragment = FragmentComponent.FromString(uriString, query.Offset);

                return new UniformResourceIdentifier
                {
                    _scheme = scheme.Scheme,
                    _authority = authority.Authority,
                    _path = path.Path,
                    _query = query.Query,
                    _fragment = fragment,
                };
            }
            catch (InvalidUriException)
            {
                return null;
            }
        }

        /// <summary>
        /// This method returns a string representation of the object.
        /// </summary>
        /// <returns>
        /// A string representation of the object is returned.
        /// </returns>
        public override string ToString()
        {
            var uriBuilder = new StringBuilder();

            _scheme?.BuildEncodedString(uriBuilder);
            _authority?.BuildEncodedString(uriBuilder);
            _path?.BuildEncodedString(uriBuilder);
            _query?.BuildEncodedString(uriBuilder);
            _fragment?.BuildEncodedString(uriBuilder);

            return uriBuilder.ToString();
        }

        /// <summary>
        /// This method returns an indication of whether or not the URI is a relative reference URI.
        /// </summary>
        public bool IsRelativeReference => HasScheme == false;

        /// <summary>
        /// This field represents the Scheme of a URI. If the Scheme component was
        /// not present in the URI, the value of this field will be null.
        /// </summary>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.2">RFC 3986 (Section 3.2)</seealso>
        private SchemeComponent _scheme;

        /// <summary>
        /// This method returns a string representation of the Scheme component.
        /// </summary>
        public string Scheme => _scheme?.Scheme;

        /// <summary>
        /// This method returns an indication of whether or not the URI has a Scheme component.
        /// </summary>
        public bool HasScheme => _scheme != null;

        /// <summary>
        /// This field represents the Authority of a URI. If the authority component was
        /// not present in the URI, the value of this field will be null.
        /// </summary>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.2">RFC 3986 (Section 3.2)</seealso>
        private AuthorityComponent _authority;

        /// <summary>
        /// This method returns a string representation of the Authority component. If the Authority component
        /// is not present in the URI, this method returns null.
        /// </summary>
        public string Authority => _authority?.Authority;

        /// <summary>
        /// This method returns an indication of whether or not the URI has an Authority component.
        /// </summary>
        public bool HasAuthority => _authority != null;

        /// <summary>
        /// This method returns a string representation of the UserInformation component of the URI. If the
        /// UserInformation component is not present in the URI, this method returns null.
        /// </summary>
        public string UserInformation => _authority?.UserInformation;

        /// <summary>
        /// This method returns an indication of whether or not the URI has an UserInformation component.
        /// </summary>
        public bool HasUserInformation => _authority?.HasUserInformation ?? false;

        /// <summary>
        /// This method returns a string representation of the Host component of the URI. If the
        /// Host component is not present in the URI, this method returns null.
        /// </summary>
        public string Host => _authority?.Host;

        /// <summary>
        /// This method returns an indication of whether or not the URI has a Host component.
        /// </summary>
        public bool HasHost => _authority?.HasHost ?? false;

        /// <summary>
        /// This method returns a string representation of the Port component of the URI. If the
        /// Port component is not present in the URI, this method returns null.
        /// </summary>
        public int Port => _authority?.Port ?? -1;

        /// <summary>
        /// This method returns an indication of whether or not the URI has a Port component.
        /// </summary>
        public bool HasPort => _authority?.HasPort ?? false;

        /// <summary>
        /// This field represents the Path component of a URI. If the component was not present
        /// in the URI, then the value of this field is null.
        /// </summary>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.3">RFC 3986 (Section 3.3)</seealso>
        private PathComponent _path;

        /// <summary>
        /// This method returns the Path of a URI. If the path component of the URI
        /// was empty this method returns a collection of zero elements.
        /// </summary>
        /// <remarks>
        /// If the path of the URI was an absolute path then the first element of this
        /// collection will be an empty string.
        /// </remarks>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.3">RFC 3986 (Section 3.3)</seealso>
        public IList<string> Path => _path.Segments;

        /// <summary>
        /// This property returns an indication of whether or not the Path component of the URI is empty.
        /// </summary>
        public bool HasEmptyPath => Path.Count == 0;

        /// <summary>
        /// This field represents the Query component of a URI. If the component was not present
        /// in the URI, then the value of this field is null.
        /// </summary>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.4">RFC 3986 (Section 3.4)</seealso>
        private QueryComponent _query;

        /// <summary>
        /// This property returns an indication of whether or not the URI has the Query component.
        /// </summary>
        public bool HasQuery => _query != null;

        /// <summary>
        /// This method returns the Query component of a URI. If the component was not present
        /// in the URI, then this method returns null.
        /// </summary>
        /// <remarks>
        /// If the Query component is present but empty, this method returns
        /// <see cref="string.Empty"/>. This component's percent-encoded characters are not decoded.
        /// In order to use Query component with decoded percent-encoded characters use <see cref="Query" />.
        /// </remarks>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.4">RFC 3986 (Section 3.4)</seealso>
        public string QueryString => _query?.QueryString;

        /// <summary>
        /// This method returns the segments the Query component of the URI parsed into key-values pairs.
        /// </summary>
        /// <returns>
        /// Inside a Query component two values can have the same key.
        /// If the URI doesn't have a Query component, this method returns null.
        /// If the URI have a Query component, but it's empty, this method returns a
        /// collection of zero elements.
        /// </returns>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.4">RFC 3986 (Section 3.4)</seealso>
        public IReadOnlyList<KeyValuePair<string, string>> Query => _query?.Segments;

        /// <summary>
        /// This field represents the Fragment component of the URI. If the URI does not have the Fragment
        /// component, then the value of this property is null.
        /// </summary>
        /// <value>
        /// If the Fragment component is present but empty, the value of this field is
        /// <seealso cref="string.Empty"/>.
        /// </value>
        /// <seealso href="https://tools.ietf.org/html/rfc3986#section-3.5">RFC 3986 (Section 3.5)</seealso>
        private FragmentComponent _fragment;

        /// <summary>
        /// This method returns a string representation of the Fragment component of the URI.
        /// </summary>
        /// <returns>
        /// A string representation of the Fragment component of the URI is returned.
        /// </returns>
        public string Fragment => _fragment?.Fragment;

        /// <summary>
        /// This property returns an indication of whether or not the URI has the Fragment component.
        /// </summary>
        public bool HasFragment => Fragment != null;

        /// <summary>
        /// This is the default constructor of the <see cref="UniformResourceIdentifier"/> class.
        /// Since this constructor is private the only was of create a new <see cref="UniformResourceIdentifier"/>
        /// object is through <see cref="FromString"/> static method.
        /// </summary>
        private UniformResourceIdentifier() { }
    }
}
