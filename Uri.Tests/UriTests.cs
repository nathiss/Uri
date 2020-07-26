#region Copyrights
// This file is a part of the SharpShare.
//
// Copyright (c) 2020 Kamil Rusin
// Licensed under the MIT License.
// See LICENSE.txt file in the project root for full license information.
#endregion

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uri;

namespace Uri.Tests
{
    [TestClass]
    public class UriTests
    {
        [DataTestMethod]
        [DataRow("https://example.com/", "https", "example.com", new[] {""})]
        [DataRow("https://user@www.example.com/", "https", "user@www.example.com", new[] {""})]
        [DataRow("https://www.example.com", "https", "www.example.com", new string[] {})]
        [DataRow("ssh://a.com/foo/bar", "ssh", "a.com", new [] {"", "foo", "bar"})]
        [DataRow("ss+h://a.com/foo/bar", "ss+h", "a.com", new [] {"", "foo", "bar"})]
        [DataRow("mailto:John.Doe@example.com", "mailto", null, new [] {"John.Doe@example.com"})]
        [DataRow("tel:+1-816-555-1212", "tel", null, new [] {"+1-816-555-1212"})]
        public void FromString_ReceivedValidUri_UriParsedProperly(
            string uriString,
            string scheme,
            string authority,
            string[] path)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsNotNull(uri);
            Assert.AreEqual(scheme, uri.Scheme);
            Assert.AreEqual(authority, uri.Authority);
            Assert.IsTrue(path.SequenceEqual(uri.Path));
        }

        [TestMethod]
        public void FromString_ReceivedNull_ReturnedNull()
        {
            // Act
            var uri = Uri.FromString(null);

            // Assert
            Assert.IsNull(uri);
        }

        [TestMethod]
        public void FromString_ReceivedEmptyString_ReturnedNull()
        {
            // Act
            var uri = Uri.FromString(string.Empty);

            // Assert
            Assert.IsNull(uri);
        }

        [TestMethod]
        public void FromString_ReceivedStringContainingOnlyWhitespaces_ReturnedNull()
        {
            // Act
            var uri = Uri.FromString("    \t    \n    \r     ");

            // Assert
            Assert.IsNull(uri);
        }

        [TestMethod]
        public void FromString_UriWithSchemeAndEmptyPath_UriProperlyParsed()
        {
            // Act
            var uri = Uri.FromString("ssh:");

            // Assert
            Assert.IsNotNull(uri);
            Assert.AreEqual("ssh", uri.Scheme);
            Assert.IsNull(uri.Authority);
            Assert.IsNotNull(uri.Path);
            Assert.AreEqual(0, uri.Path.Count);
        }

        [DataTestMethod]
        [DataRow("ssh://auth.xyz//foo")]
        [DataRow("ssh://auth.xyz//foo/bar")]
        [DataRow("ssh://auth.xyz//")]
        public void FromString_PathAbsoluteAndFirstSegmentEmpty_ReturnedNull(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsNull(uri);
        }

        [TestMethod]
        public void FromString_SchemeWithFirstCharNotLetter_ReturnedNull()
        {
            // Act
            var uri = Uri.FromString("1sh://auth.xyz//foo");

            // Assert
            Assert.IsNull(uri);
        }

        [DataTestMethod]
        [DataRow("ssh(://auth.xyz/")]
        [DataRow("s&sh://auth.xyz/")]
        [DataRow("s@sh://auth.xyz/")]
        [DataRow("s/sh://auth.xyz/")]
        public void FromString_SchemeWithNotAllowedChar_ReturnedNull(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsNull(uri);
        }

        [DataTestMethod]
        [DataRow("ssh::/auth.xyz/")]
        [DataRow("ssh:://auth.xyz/")]
        [DataRow("ssh:abc:/auth.xyz/")]
        public void FromString_NoAuthorityFirstPathWithColon_ReturnedNull(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsNull(uri);
        }

        [TestMethod]
        public void FromString_AuthorityEqualToHost_ReturnedValidObject()
        {
            // Act
            var uri = Uri.FromString("https://www.example.com/foo/bar");

            // Assert
            Assert.AreEqual(uri.Authority, uri.Host);
        }

        [TestMethod]
        public void FromString_UserInformationNotEmpty_ReturnedValidObject()
        {
            // Act
            var uri = Uri.FromString("https://user@www.example.com/foo/bar");

            // Assert
            Assert.IsNotNull(uri);
            Assert.AreEqual("user", uri.UserInformation);
        }

        [TestMethod]
        public void FromString_UserInformationNotEmpty_ValidHostProperty()
        {
            // Act
            var uri = Uri.FromString("https://user@www.example.com/foo/bar");

            // Assert
            Assert.IsNotNull(uri);
            Assert.AreEqual("www.example.com", uri.Host);
        }

        [TestMethod]
        public void FromString_HostWithMixedCapitalization_LowerCaseHost()
        {
            // Act
            var uri = Uri.FromString("https://user@wWw.eXaMPle.COm/foo/bar");

            // Assert
            Assert.IsNotNull(uri);
            Assert.AreEqual("www.example.com", uri.Host);
        }

        [DataTestMethod]
        [DataRow("HTTPS://a.com", "https")]
        [DataRow("HttpS://a.com", "https")]
        [DataRow("hTTpS://a.com", "https")]
        [DataRow("hTtPs://a.com", "https")]
        public void FromString_SchemeWithMixedCapitalization_LowerCaseScheme(string uriString, string scheme)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsNotNull(uri);
            Assert.AreEqual(scheme, uri.Scheme);
        }

        [TestMethod]
        public void FromString_PathWithMixedCapitalization_CapitalizationIsPreserved()
        {
            // Act
            var uri = Uri.FromString("ssh://example.com/FoO/BAR");

            // Assert
            Assert.AreEqual("FoO", uri.Path[1]);
            Assert.AreEqual("BAR", uri.Path[2]);
        }

        [TestMethod]
        public void FromString_UserInformationWithMixedCapitalization_CapitalizationIsPreserved()
        {
            // Act
            var uri = Uri.FromString("ssh://ABcdEfG@example.com/FoO/BAR");

            // Assert
            Assert.AreEqual("ABcdEfG", uri.UserInformation);
        }

        [TestMethod]
        public void FromString_UserInformationEmpty_ReturnedValidObject()
        {
            // Act
            var uri = Uri.FromString("ssh://@example.com/FoO/BAR");

            // Assert
            Assert.IsNotNull(uri);
            Assert.AreEqual("", uri.UserInformation);
        }

        [DataTestMethod]
        [DataRow("ssh://us$%er@example.com/FoO/BAR", "us$%er")]
        [DataRow("ssh://us'er@example.com/FoO/BAR", "us'er")]
        [DataRow("ssh://u:s~er@example.com/FoO/BAR", "u:s~er")]
        public void FromString_UserInformationExtraChars_ReturnedValidObject(string uriString, string userInformation)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.AreEqual(userInformation, uri.UserInformation);
        }

        [DataTestMethod]
        [DataRow("s¿sh://us$%er@exæample.com/FoO/BAR")]
        [DataRow("ssh://usą'er@example.com/FoO/BAR")]
        [DataRow("ssh://u:s~er@example.com/FoäO/BAR")]
        [DataRow("ssh://u:s~er@example.com/FoąO/BAR")]
        [DataRow("ssh://@example.com/FoO/BAR҂")]
        public void FromString_UriContainsUnicode_ReturnsNull(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsNull(uri);
        }

        [TestMethod]
        public void FromString_UriWithPort_ReturnedValidObject()
        {
            // Act
            var uri = Uri.FromString("https://example.com:8080/");

            // Assert
            Assert.AreEqual(8080, uri.Port);
        }

        [TestMethod]
        public void FromString_UriWithEmptyPort_ReturnedValidObject()
        {
            // Act
            var uri = Uri.FromString("https://example.com:/");

            // Assert
            Assert.AreEqual(0, uri.Port);
        }

        [TestMethod]
        public void FromString_PortWithNonDigit_ReturnsNull()
        {
            // Act
            var uri = Uri.FromString("https://example.com:80a/");

            // Assert
            Assert.IsNull(uri);
        }

        [TestMethod]
        public void FromString_AuthorityWithPort_ReturnsNull()
        {
            // Act
            var uri = Uri.FromString("https://user@example.com:80/");

            // Assert
            Assert.IsNotNull(uri);
            Assert.AreEqual("user@example.com:80", uri.Authority);
            Assert.AreEqual("user", uri.UserInformation);
            Assert.AreEqual("example.com", uri.Host);
            Assert.AreEqual(80, uri.Port);
        }

        [TestMethod]
        public void FromString_UserWithColonAndPortPresent_ReturnedValidObject()
        {
            // Act
            var uri = Uri.FromString("https://user:passwd@example.com:8080/foo");

            // Assert
            Assert.IsNotNull(uri);
            Assert.AreEqual("user:passwd@example.com:8080", uri.Authority);
            Assert.AreEqual("user:passwd", uri.UserInformation);
            Assert.AreEqual("example.com", uri.Host);
            Assert.AreEqual(8080, uri.Port);
        }

        [TestMethod]
        public void FromString_SchemeWithoutEndingColon_ReturnsNull()
        {
            // Act
            var uri = Uri.FromString("https");

            // Assert
            Assert.IsNull(uri);
        }

        [TestMethod]
        public void FromString_AuthorityMixedCapitalization_AuthorityNormalizedAndHostNormalized()
        {
            // Act
            var uri = Uri.FromString("ssh://USER:Passwd@EXAMple.UK.com/");

            // Assert
            Assert.AreEqual("USER:Passwd@example.uk.com", uri.Authority);
            Assert.AreEqual("USER:Passwd", uri.UserInformation);
            Assert.AreEqual("example.uk.com", uri.Host);
        }

        [DataTestMethod]
        [DataRow("https://example.com/")]
        [DataRow("https:example/:com")]
        [DataRow("a:")]
        public void HasScheme_UriHasScheme_ReturnsTrue(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsTrue(uri.HasScheme);
        }

        [DataTestMethod]
        [DataRow("https://example.com/")]
        [DataRow("https://example:12")]
        [DataRow("a://a")]
        public void HasAuthority_UriHasAuthority_ReturnsTrue(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsTrue(uri.HasAuthority);
        }

        [DataTestMethod]
        [DataRow("https:example/com/foo")]
        [DataRow("https:")]
        [DataRow("a:#Fragment")]
        public void HasAuthority_UriDoesNotHaveAuthority_ReturnsFalse(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsFalse(uri.HasAuthority);
        }

        [DataTestMethod]
        [DataRow("https://user@example.com/")]
        [DataRow("https://yser:passwd@example:12")]
        [DataRow("a://@a")]
        public void HasUserInformation_UriHasUserInformation_ReturnsTrue(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsTrue(uri.HasUserInformation);
        }

        [DataTestMethod]
        [DataRow("https://example.com/")]
        [DataRow("https://yser.passwd.example:12")]
        [DataRow("a://a")]
        public void HasUserInformation_UriDoesNotHaveUserInformation_ReturnsFalse(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsFalse(uri.HasUserInformation);
        }

        [DataTestMethod]
        [DataRow("https://user@example.com:/")]
        [DataRow("https://yser:passwd@a:12")]
        [DataRow("a://a")]
        public void HasHost_UriHasHost_ReturnsTrue(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsTrue(uri.HasHost);
        }

        [DataTestMethod]
        [DataRow("https:user")]
        [DataRow("https:#Fragment")]
        [DataRow("a:")]
        public void HasHost_UriDoesNotHaveHost_ReturnsFalse(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsFalse(uri.HasHost);
        }

        [DataTestMethod]
        [DataRow("https://yser:passwd@a:12")]
        public void HasPort_UriHasPort_ReturnsTrue(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsTrue(uri.HasPort);
        }

        [DataTestMethod]
        [DataRow("https://user@example.com/")]
        [DataRow("https://yser:passwd@a")]
        [DataRow("a://@a")]
        [DataRow("a://a:")]
        [DataRow("https://user@example.com:/")]
        public void HasPort_UriDoesNotHavePort_ReturnsFalse(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsFalse(uri.HasPort);
        }

        [DataTestMethod]
        [DataRow("https://user@example.com:12")]
        [DataRow("https://yser:passwd@a:12")]
        [DataRow("a://a")]
        public void HasEmptyPath_UriHasEmptyPath_ReturnsTrue(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsTrue(uri.HasEmptyPath);
        }

        [DataTestMethod]
        [DataRow("https://user@example.com:12/")]
        [DataRow("https://yser:passwd@a:12/foo")]
        [DataRow("a:a")]
        [DataRow("a://a/foo/bar/")]
        [DataRow("a:sdffd/")]
        public void HasEmptyPath_UriHasNonEmptyPath_ReturnsFalse(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsFalse(uri.HasEmptyPath);
        }

        [TestMethod]
        public void FromString_UserInformationHasNotAllowedChars_ReturnsNull()
        {
            // Act
            var uri = Uri.FromString("https://user:pass[wd@example.com/");

            // Assert
            Assert.IsNull(uri);
        }

        [TestMethod]
        public void FromString_WithAuthorityPathContainsNotAllowedChars_ReturnsNull()
        {
            // Act
            var uri = Uri.FromString("https://exmaple.com/foo[/bar");

            // Assert
            Assert.IsNull(uri);
        }

        [TestMethod]
        public void FromString_WithoutAuthorityPathContainsNotAllowedChars_ReturnsNull()
        {
            // Act
            var uri = Uri.FromString("https:foo]/bar");

            // Assert
            Assert.IsNull(uri);
        }

        [DataTestMethod]
        [DataRow("https://user:@exam{ple.com/")]
        [DataRow("https://user:@exam<ple.com/")]
        public void FromString_HostHasNotAllowedChars_ReturnsNull(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsNull(uri);
        }

        [DataTestMethod]
        [DataRow("ftp://[v76.example.com]/", "[v76.example.com]", "v76.example.com")]
        [DataRow("ftp://[v76.exam-ple.co~m]", "[v76.exam-ple.co~m]", "v76.exam-ple.co~m")]
        [DataRow("ftp://[V76.exam-ple.co~m]", "[v76.exam-ple.co~m]", "v76.exam-ple.co~m")]
        [DataRow("ftp://user:passwd@[v76.e:x:a:m:p:l:e.com]:8080",
            "user:passwd@[v76.e:x:a:m:p:l:e.com]:8080",
            "v76.e:x:a:m:p:l:e.com")
        ]
        [DataRow("ftp://user:passwd@[vAB.e:x:a:m:p:l:e.com]:8080",
            "user:passwd@[vab.e:x:a:m:p:l:e.com]:8080",
            "vab.e:x:a:m:p:l:e.com")
        ]
        [DataRow("ftp://user:passwd@[VAB.e:x:a:m:p:l:e.com]:8080",
            "user:passwd@[vab.e:x:a:m:p:l:e.com]:8080",
            "vab.e:x:a:m:p:l:e.com")
        ]
        public void FromString_ValidIpLiteralIpVFuture_ReturnsValidHostAndAuthority(
            string uriString,
            string authority,
            string host)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsNotNull(uri);
            Assert.AreEqual(authority, uri.Authority);
            Assert.AreEqual(host, uri.Host);
        }

        [DataTestMethod]
        [DataRow("https://[v76.example.com")]
        [DataRow("https://[v76.example.com/")]
        [DataRow("https://[v76.example.com:8080/")]
        [DataRow("https://[v76.example.com:8080")]
        [DataRow("https://user@[v76.example.com:8080")]
        [DataRow("https://user@[1111:2222:3333:4444:5555:6666:7777:8888")]
        [DataRow("https://user@[1111:2222:3333:4444:5555:6666:7777:8888/")]
        [DataRow("https://user@[1111:2222:3333::7777:8888/")]
        public void FromString_InvalidIpLiteral_ReturnsNull(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsNull(uri);
        }

        [DataTestMethod]
        [DataRow("ftp://[1111:2222:3333:4444:5555:6666:7777:8888]/",
            "[1111:2222:3333:4444:5555:6666:7777:8888]",
            "1111:2222:3333:4444:5555:6666:7777:8888")
        ]
        [DataRow("https://[1111:2222:3333:4444:5555:6666:7777:8888]/foo/bat",
            "[1111:2222:3333:4444:5555:6666:7777:8888]",
            "1111:2222:3333:4444:5555:6666:7777:8888")
        ]
        [DataRow("ftp://[1:22:3333:4444:555:66:7777:88]",
            "[1:22:3333:4444:555:66:7777:88]",
            "1:22:3333:4444:555:66:7777:88")
        ]
        [DataRow("ftp://[1:2a2:3333:4444:5BB5:66:eeFF:88]",
            "[1:2a2:3333:4444:5bb5:66:eeff:88]",
            "1:2a2:3333:4444:5bb5:66:eeff:88")
        ]
        [DataRow("ftp://user:passwd@[1:2a2:3333:4444:5BB5:66:eeFF:88]:8080",
            "user:passwd@[1:2a2:3333:4444:5bb5:66:eeff:88]:8080",
            "1:2a2:3333:4444:5bb5:66:eeff:88")
        ]
        [DataRow("ftp://user:passwd@[1:2a2:0:004:0000:00:eeFF:88]:8080",
            "user:passwd@[1:2a2:0:4:0:0:eeff:88]:8080",
            "1:2a2:0:4:0:0:eeff:88")
        ]
        [DataRow("ftp://user:passwd@[1::0eff:88]:8080",
            "user:passwd@[1::eff:88]:8080",
            "1::eff:88")
        ]
        [DataRow("ftp://user:passwd@[1::]:8080",
            "user:passwd@[1::]:8080",
            "1::")
        ]
        [DataRow("ftp://user:passwd@[::1]:8080",
            "user:passwd@[::1]:8080",
            "::1")
        ]
        [DataRow("ftp://user:passwd@[::]:8080",
            "user:passwd@[::]:8080",
            "::")
        ]
        public void FromString_ValidIpV6Address_ReturnsValidHostAndAuthority(
            string uriString,
            string authority,
            string host)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsNotNull(uri);
            Assert.AreEqual(authority, uri.Authority);
            Assert.AreEqual(host, uri.Host);
        }

        [DataTestMethod]
        [DataRow("ftp://[1111:2222:33333:4444:5555:6666:7777:8888]/")]
        [DataRow("ftp://[:2222:3333:4444:5555:6666:7777:8888]/")]
        [DataRow("ftp://[1:2222:3333:4444:5555:6666:7777:]/")]
        [DataRow("ftp://[11:2222:::5555:6666:7777:8888]/")]
        [DataRow("ftp://[:22:3333:4444:555:66:7777:88]")]
        [DataRow("ftp://[1:00000:3333:4444:5BB5:66:eeFF:88]")]
        [DataRow("ftp://user:passwd@[:]:8080")]
        [DataRow("ftp://user:passwd@[1::0:004:0000::eeFF:88]:8080")]
        [DataRow("ftp://user:passwd@[1::34G:88]:8080")]
        public void FromString_InValidIpV6Address_ReturnsNull(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsNull(uri);
        }

        [DataTestMethod]
        [DataRow("ftp://127.0.0.1/", "127.0.0.1", "127.0.0.1")]
        [DataRow("ftp://255.255.255.255/", "255.255.255.255", "255.255.255.255")]
        [DataRow("ftp://user.passwd@255.255.255.255/", "user.passwd@255.255.255.255", "255.255.255.255")]
        [DataRow("ftp://@255.255.255.255:8080/", "@255.255.255.255:8080", "255.255.255.255")]
        [DataRow("ftp://@0.0.0.0:8080/", "@0.0.0.0:8080", "0.0.0.0")]
        public void FromString_ValidIpV4Address_ReturnsValidHostAndAuthority(
            string uriString,
            string authority,
            string host)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsNotNull(uri);
            Assert.AreEqual(authority, uri.Authority);
            Assert.AreEqual(host, uri.Host);
        }

        [DataTestMethod]
        [DataRow("ftp://[127.0.0.1]/")]
        public void FromString_InValidIpV4Address_ReturnsNull(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsNull(uri);
        }

        [DataTestMethod]
        [DataRow("unix:///var/docker.sock", "unix", "", "", new [] {"", "var", "docker.sock"})]
        public void FromString_ValidUnixSocket_ReturnsValidObject(
            string uriString,
            string scheme,
            string authority,
            string host,
            string[] path)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsNotNull(uri);
            Assert.AreEqual(scheme, uri.Scheme);
            Assert.AreEqual(authority, uri.Authority);
            Assert.AreEqual(host, uri.Host);
            Assert.IsFalse(uri.HasPort);
            Assert.IsTrue(path.SequenceEqual(uri.Path));
        }

        [DataTestMethod]
        [DataRow("https://www.example.com/index.html?query", "query")]
        [DataRow("https://www.example.com/index.html?key=value", "key=value")]
        [DataRow("https://www.example.com/index.html?key=value#Fragment", "key=value")]
        [DataRow("https://www.example.com/index.html?key=v/a/l/u/e#Fragment", "key=v/a/l/u/e")]
        [DataRow("https://www.example.com/index.html?", "")]
        [DataRow("https://www.example.com/index.html?#", "")]
        [DataRow("https://www.example.com/index.html??#", "?")]
        [DataRow("https://www.example.com/index.html?key=?value#", "key=?value")]
        public void FromString_UriWithQueryComponent_ReturnsValidObject(
            string uriString,
            string query)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsNotNull(uri);
            Assert.IsTrue(uri.HasQuery);
            Assert.AreEqual(query, uri.QueryString);
        }

        [DataTestMethod]
        [DataRow("tel:+14-456-312-455?[")]
        [DataRow("https://example.com/path/?key=[")]
        [DataRow("https://example.com/path/?key=val\"ue")]

        public void FromString_QueryWithNotAllowedChars_ReturnsNull(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsNull(uri);
        }

        [TestMethod]
        public void FromString_UriWithEmptyQuery_ReturnsValidObject()
        {
            // Act
            var uri = Uri.FromString("https://example.com?");

            // Assert
            Assert.IsTrue(uri.HasQuery);
            Assert.AreEqual(string.Empty, uri.QueryString);
            Assert.AreEqual(0, uri.Query.Count);
        }

        [TestMethod]
        public void FromString_UriKeyValueQuery_ReturnsValidObject()
        {
            // Act
            var uri = Uri.FromString("https://example.com?key1=value1&key2=value2#key=value3");

            // Assert
            Assert.IsTrue(uri.HasQuery);
            Assert.AreEqual("key1=value1&key2=value2", uri.QueryString);
            Assert.AreEqual(2, uri.Query.Count);
            Assert.AreEqual("key1", uri.Query[0].Key);
            Assert.AreEqual("value1", uri.Query[0].Value);
            Assert.AreEqual("key2", uri.Query[1].Key);
            Assert.AreEqual("value2", uri.Query[1].Value);
        }

        [TestMethod]
        public void FromString_QueryWithOnlyKeys_ReturnsValidObject()
        {
            // Act
            var uri = Uri.FromString("https://example.com?key1&key2#key3");

            // Assert
            Assert.IsTrue(uri.HasQuery);
            Assert.AreEqual("key1&key2", uri.QueryString);
            Assert.AreEqual(2, uri.Query.Count);
            Assert.AreEqual("key1", uri.Query[0].Key);
            Assert.AreEqual(null, uri.Query[0].Value);
            Assert.AreEqual("key2", uri.Query[1].Key);
            Assert.AreEqual(null, uri.Query[0].Value);
        }

        [TestMethod]
        public void FromString_QueryWithKeysAndEmptyValues_ReturnsValidObject()
        {
            // Act
            var uri = Uri.FromString("https://example.com?key1=&key2=#key3=");

            // Assert
            Assert.IsTrue(uri.HasQuery);
            Assert.AreEqual("key1=&key2=", uri.QueryString);
            Assert.AreEqual(2, uri.Query.Count);
            Assert.AreEqual("key1", uri.Query[0].Key);
            Assert.AreEqual(string.Empty, uri.Query[0].Value);
            Assert.AreEqual("key2", uri.Query[1].Key);
            Assert.AreEqual(string.Empty, uri.Query[0].Value);
        }

        [DataTestMethod]
        [DataRow("https://example.com/path?key=value")]
        [DataRow("https://example.com/path?key")]
        [DataRow("https://example.com/path?key=")]
        [DataRow("https://example.com/path?key1&key2")]
        [DataRow("https://example.com/path?")]
        [DataRow("https://example.com/path?#")]
        [DataRow("https://example.com/path??#")]
        [DataRow("https://example.com/path?=value#")]
        [DataRow("https:///path?=value")]
        [DataRow("x:path?=value")]
        [DataRow("x:path?#")]
        public void HasQuery_UriHasQueryComponent_ReturnsTrue(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsTrue(uri.HasQuery);
        }

        [DataTestMethod]
        [DataRow("https:")]
        [DataRow("https://user&passwd@example.com/#")]
        [DataRow("https://user&passwd@example.com/")]
        [DataRow("https://user&passwd@exam&ple.com/")]
        [DataRow("https://user&passwd@exam&ple.com:8080/")]
        [DataRow("https://user&passwd@exam&ple.com:/#")]
        [DataRow("https://user&passwd@exam&ple.com:8080/#")]
        public void HasQuery_UriDoesNotHaveQueryComponent_ReturnsFalse(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsFalse(uri.HasQuery);
        }

        [DataTestMethod]
        [DataRow("https://example.com/path?key1=value1#Fragment", "Fragment")]
        [DataRow("https://example.com/path#Fragment", "Fragment")]
        [DataRow("https:///#foo", "foo")]
        [DataRow("https:///#bar?", "bar?")]
        [DataRow("https:///#/", "/")]
        [DataRow("https:///#", "")]
        [DataRow("a:///#path/foo", "path/foo")]
        [DataRow("a://example.com/path#?key=value", "?key=value")]
        [DataRow("a://example.com/path#", "")]
        public void FromString_UriHasFragment_ReturnsValidObject(
            string uriString,
            string fragment)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.AreEqual(fragment, uri.Fragment);
        }


        [DataTestMethod]
        [DataRow("https://example.com/path?key1=value1#Fragment")]
        [DataRow("https://example.com/path#Fragment")]
        [DataRow("https:///#foo")]
        [DataRow("https:///#bar?")]
        [DataRow("https:///#/")]
        [DataRow("https:///#")]
        [DataRow("a:///#path/foo")]
        [DataRow("a://example.com/path#?key=value")]
        [DataRow("a://example.com/path#")]
        public void HasFragment_UriHasFragment_HasFragmentReturnsTrue(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsTrue(uri.HasFragment);
        }

        [DataTestMethod]
        [DataRow("https:///")]
        [DataRow("a:///path/foo")]
        [DataRow("a://example.com/path?key=value")]
        [DataRow("a://example.com/path")]
        public void FromString_UriDoesNotHaveFragment_ReturnsValidObject(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.AreEqual(null, uri.Fragment);
        }

        [DataTestMethod]
        [DataRow("https:///")]
        [DataRow("a:///path/foo")]
        [DataRow("a://example.com/path?key=value")]
        [DataRow("a://example.com/path")]
        public void HasFragment_UriDoesNotHaveFragment_HasFragmentReturnsFalse(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsFalse(uri.HasFragment);
        }

        [DataTestMethod]
        [DataRow("tel:+14-456-312-455?#[")]
        [DataRow("https://example.com/path/?key=#asf[")]
        [DataRow("https://example.com/path/?#key=val\"ue")]
        [DataRow("https://example.com/path/?#key=val{ue")]

        public void FromString_FragmentWithNotAllowedChars_ReturnsNull(string uriString)
        {
            // Act
            var uri = Uri.FromString(uriString);

            // Assert
            Assert.IsNull(uri);
        }
    }
}
