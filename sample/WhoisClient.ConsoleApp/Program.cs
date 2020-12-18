using System;
using System.Threading.Tasks;

namespace WhoisClient.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new WhoisClient();
            do
            {
                Console.WriteLine("Write your domain:");
                //google.com
                var input = Console.ReadLine();
                try
                {
                    var server = await client.LookupAsync(input!);
                    Console.WriteLine(server);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                Console.WriteLine();
            } while (true);
        }
    }
}
