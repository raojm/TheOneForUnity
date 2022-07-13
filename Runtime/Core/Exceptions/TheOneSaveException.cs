using System;

namespace TheOneUnity.Core.Exceptions
{
    public class TheOneSaveException : Exception
    {
        public TheOneSaveException() { }

        public TheOneSaveException(string message) : base(message) { }
    }
}