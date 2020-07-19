# Universal Resource Identifier

A Uniform Resource Identifier (URI) is a compact sequence of characters that identifies
an abstract or physical resource.
URI specification is described in [RFC 3986](https://tools.ietf.org/html/rfc3986).

## Syntax

[RFC 3986 (Section 3)](https://tools.ietf.org/html/rfc3986#section-3)

URI         = `scheme ":" hier-part [ "?" query ] [ "#" fragment ]`

hier-part   = `"//" authority path-abempty`
            `/ path-absolute`
            `/ path-rootless`
            `/ path-empty`

```
     foo://example.com:8042/over/there?name=ferret#nose
     \_/   \______________/\_________/ \_________/ \__/
      |           |            |            |        |
   scheme     authority       path        query   fragment
      |   _____________________|__
     / \ /                        \
     urn:example:animal:ferret:nose
```

The scheme and path components are required, though the path may be empty
(no characters). When authority is present, the path must either be empty
or begin with a slash ("/") character.  When authority is not present,
the path cannot begin with two slash characters ("//").

For more detailed information about URI components see:
* **Scheme:** [RFC 3986 (Section 3.1)](https://tools.ietf.org/html/rfc3986#section-3.1)
* **Authority:** [RFC 3986 (Section 3.2)](https://tools.ietf.org/html/rfc3986#section-3.2)
  * **User Information:** [RFC 3986 (Section 3.2.1)](https://tools.ietf.org/html/rfc3986#section-3.2.1)
  * **Host:** [RFC 3986 (Section 3.2.2)](https://tools.ietf.org/html/rfc3986#section-3.2.2)
  * **Port:** [RFC 3986 (Section 3.2.3)](https://tools.ietf.org/html/rfc3986#section-3.2.3)
* **Path:** [RFC 3986 (Section 3.3)](https://tools.ietf.org/html/rfc3986#section-3.3)
* **Query:** [RFC 3986 (Section 3.4)](https://tools.ietf.org/html/rfc3986#section-3.4)
* **Fragment:** [RFC 3986 (Section 3.5)](https://tools.ietf.org/html/rfc3986#section-3.5)

### Percent-Encoding

[RFC 3986 (Section 2.1)](https://tools.ietf.org/html/rfc3986#section-2.1)

A percent-encoding mechanism is used to represent a data octet in a
component when that octet's corresponding character is outside the
allowed set or is being used as a delimiter of, or within, the
component.  A percent-encoded octet is encoded as a character
triplet, consisting of the percent character "%" followed by the two
hexadecimal digits representing that octet's numeric value.  For
example, `"%20"` is the percent-encoding for the binary octet
`"00100000`" (ABNF: `%x20`), which in US-ASCII corresponds to the space
character (SP).

#### Reserved sets of characters

The following character must be percent-encoded if they are a part of data:
* `":", "/", "?", "#", "[", "]", "@"`
* `"!", "$", "&", "'", "(", ")", "*", "+", ",", ";", "="`

#### Unreserved set of characters

The following characters do not need to be percent-encoded:
* `ALPHA, DIGIT, "-", ".", "_", "~"`

*Note:
URIs that differ in the replacement of an unreserved character with its
corresponding percent-encoded US-ASCII octet are equivalent.
*

### Examples of URIs

The following example of URIs illustrate several URI schemes and variations in
their common syntax components:

* ftp://ftp.is.co.za/rfc/rfc1808.txt
* http://www.ietf.org/rfc/rfc2396.txt
* ldap://[2001:db8::7]/c=GB?objectClass?one
* mailto:John.Doe@example.com
* news:comp.infosystems.www.servers.unix
* tel:+1-816-555-1212
* telnet://192.0.2.16:80/
* urn:oasis:names:specification:docbook:dtd:xml:4.1.2

## Notes

* The scheme and path components are required, though the path may be
  empty (no characters).
* The syntax rule for host is ambiguous because it does not completely
  distinguish between an IPv4address and a reg-name.  In order to
  disambiguate the syntax, we apply the "first-match-wins" algorithm:
  If host matches the rule for IPv4address, then it should be
  considered an IPv4 address literal and not a reg-name.
