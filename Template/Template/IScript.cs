namespace Template
{
    using System;
    using System.IO;

    internal interface IScript : IDisposable
    {
        void Run(TextWriter output);
    }
}