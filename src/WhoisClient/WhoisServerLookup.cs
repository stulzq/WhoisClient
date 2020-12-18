using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WhoisClient
{
    public class WhoisServerLookup
    {
        private readonly Dictionary<string, string> _tldCache;

        private const string IANA = "whois.iana.org";
        // ReSharper disable once InconsistentNaming
        private const int IANA_PORT = 43;

        public WhoisServerLookup()
        {
            _tldCache = new Dictionary<string, string>();
        }

        public async Task<string> RequestAsync(string domain, bool useCache = true)
        {
            var task = ExecuteAsync(domain, useCache);
            if (task == await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(WhoisClientOptions.WhoisServerLookupTimeout))))
            {
                return await task;
            }

            throw new TimeoutException();
        }

        private async Task<string> ExecuteAsync(string domain, bool useCache = true)
        {
            var tld = GetTld(domain).ToUpper();
            if (useCache && _tldCache.ContainsKey(tld))
            {
                return _tldCache[tld];
            }

            try
            {
                using var tcpClient = new TcpClient(IANA, IANA_PORT);
                await using var netStream = tcpClient.GetStream();
                var data = Encoding.UTF8.GetBytes(tld + "\r\n");
                await netStream.WriteAsync(data, 0, data.Length);
                using var reader = new StreamReader(netStream);
                var resp = await reader.ReadToEndAsync();
                if (resp == null)
                {
                    throw new WhoisClientException($"Connect to {IANA} success, but nothing to receive.");
                }

                var contentArr = resp.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var whoisRow = contentArr.FirstOrDefault(a => a.StartsWith("whois:"));

                if (whoisRow == null)
                {
                    throw new WhoisClientException($"Can not find whois information, maybe the Top Level Domain {tld} is not correct.");
                }

                var server = whoisRow[6..].Trim();

                if (_tldCache.ContainsKey(tld))
                {
                    _tldCache[tld] = server;
                }
                else
                {
                    _tldCache.Add(tld, server);
                }

                return server;
            }
            catch (Exception e)
            {
                throw new WhoisClientException("Lookup whois server failed.", e);
            }
        }

        private string GetTld(string domain)
        {
            var tld = domain;

            if (string.IsNullOrEmpty(domain)) return tld;
            var parts = domain.Split('.');

            if (parts.Length > 1) tld = parts[^1];

            return tld;
        }
    }
}