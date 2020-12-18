using System;

namespace WhoisClient
{
    public class WhoisClientException : Exception
    {
        public WhoisClientException(string message) : base(message)
        {

        }

        public WhoisClientException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}