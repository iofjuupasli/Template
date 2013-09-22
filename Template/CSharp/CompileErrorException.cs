namespace CSharp
{
    using System;

    public class CompileErrorException : Exception
    {
        public CompileErrorException(string message)
            : base(message)
        {
        }
    }
}