﻿namespace Template
{
    using System;
    using System.IO;

    public interface IScript : IDisposable
    {
        void Run(params object[] values);
    }
}