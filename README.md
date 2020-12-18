# WhoisClient

[![Latest version](https://img.shields.io/nuget/v/WhoisClient.svg)](https://www.nuget.org/packages/WhoisClient/) 

A simple domain whois lookup library.

## Usage

````csharp
var client = new WhoisClient();
var whoisInfo = await client.LookupAsync("google.com");
````

![image-20201218172736581](assets/image-20201218172736581.png)

## Reference

- https://github.com/flipbit/whois

- https://tools.ietf.org/html/rfc3912
