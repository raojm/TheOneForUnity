using System;

namespace TheOneUnity.Core.Exceptions
{
    public class TheOneSignupException : Exception
    {
        public TheOneSignupException() { }

        public TheOneSignupException(string message) : base(message) { }
    }
}