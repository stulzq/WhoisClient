using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WhoisClient
{
    public class WhoisClient
    {
        private readonly WhoisServerLookup _serverLookup;
        private const string DomainReg = @"^[a-zA-Z0-9][-a-zA-Z0-9]{0,62}(\.[a-zA-Z0-9][-a-zA-Z0-9]{0,62})+\.?$";
        public WhoisClient()
        {
            _serverLookup = new WhoisServerLookup();
        }

        public async Task<string> LookupAsync(string domain)
        {
            var task = ExecuteAsync(domain);
            if (task == await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(WhoisClientOptions.DomainLookupTimeout))))
            {
                return await task;
            }

            throw new TimeoutException();

        }

        public async Task<string> ExecuteAsync(string domain)
        {
            if (!ValidateDomain(domain))
            {
                throw new ArgumentException("The domain is incorrect, for example: google.com.");
            }

            var validDomain = domain;
            var arr = validDomain.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length > 2)
            {
                validDomain = $"{arr[^2]}.{arr[^1]}";
            }

            var server = await _serverLookup.RequestAsync(validDomain);
            try
            {
                using var tcpClient = new TcpClient(server, 43);
                await using var netStream = tcpClient.GetStream();
                var data = Encoding.UTF8.GetBytes(validDomain + "\r\n");
                await netStream.WriteAsync(data, 0, data.Length);
                using var reader = new StreamReader(netStream);
                var resp = await reader.ReadToEndAsync();

                if (resp == null)
                {
                    throw new WhoisClientException($"Connect to whois server {server} success, but nothing to receive.");
                }

                return resp;
            }
            catch (Exception e)
            {
                throw new WhoisClientException("An exception occurred while getting whois information.", e);
            }
        }


        public bool ValidateDomain(string domain)
        {
            return Regex.IsMatch(domain, DomainReg);
        }
    }
}