# SharpShare

[![Build Status](https://travis-ci.com/nathiss/SharpShare.svg?token=59Xn8z5MdPHEudCq8gJ3&branch=master)](https://travis-ci.com/nathiss/SharpShare)

This program is a HTTP/2 server written in C# and .NET Core 3.1.  
HTTP/2 is a extended version of the HTTP/1.1 protocol. Both share the same schematics (methods, status codes, header fields), but
HTTP/2 encodes the headers and data into a binary format, which reduces the amount of network traffic generated by the server and
clients.

## New mechanisms 

* **Streams** - The protocol allows to multiplex many bidirectional flows of data refered to as **streams**. The streams can carry
                the smallest unit of data exchange - a **frame**. Each frame is a part of a **message** which maps to a logical
                HTTP request or response.
* **Server push** - The HTTP/2 protocol allows the server to send multiple responses to a single client's request. In addition to
                    the requested resource the server can send other resources before the client requests for any of them.
                    The client can decline the push resource send by the server via a `RST_STREAM` frame.
* **Header compression** - The protocol HTTP headers to be encoded via a static Huffman code, therefore reducing their transfer size.
                           The specification also requires that both the client and server maintain and update an indexed list of
                           previously seen header fields. Which is used to efficiently encode previously transmitted values.


## References

* [RFC 7540](https://tools.ietf.org/html/rfc7540) - This is the official documentation of the protocol HTTP version 2 (HTTP/2).
* [Introduction to HTTP/2](https://developers.google.com/web/fundamentals/performance/http2) - This article written by Google is
  a nice introduction to the fundamentals of HTTP/2.
* [RFC 2616](https://tools.ietf.org/html/rfc2616) - THis is the official documentation of the protocol HTTP/1.1.
* [RFC 3986](https://tools.ietf.org/html/rfc3986) - This is the official documentation of the Universal Resource Identifier (URI).

## Licence

This program is distributed under The MIT License. See [LICENSE.txt](LICENSE.txt) file.
