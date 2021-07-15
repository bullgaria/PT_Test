using System;

namespace PT_Test.Handlers
{
    public class InvalidPartException : Exception
    {
        public InvalidPartException()
        {
        }

        public InvalidPartException(string message) : base(message)
        {
        }

        public InvalidPartException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
