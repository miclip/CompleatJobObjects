﻿using System;
using System.Collections.Generic;
using System.IO;

namespace ProcessMonitor.Utilities
{
    // BR: Add Stop(timeout).
    internal interface IProcess : IDisposable
    {
        int ExitCode { get; }
        IntPtr Handle { get; }
        int Id { get; }
        IReadOnlyDictionary<string, string> Environment { get; }

        long PrivateMemoryBytes { get; }

        event EventHandler Exited;
        event EventHandler<ProcessDataReceivedEventArgs> OutputDataReceived;
        event EventHandler<ProcessDataReceivedEventArgs> ErrorDataReceived;

        TextReader StandardOutput { get; }
        TextReader StandardError { get; }
        TextWriter StandardInput { get; }

        void Kill();
        void WaitForExit();
        bool WaitForExit(int milliseconds);
    }
}
