using System;

namespace ProcessMonitor.Utilities
{
    internal class ProcessDataReceivedEventArgs : EventArgs
    {
        public ProcessDataReceivedEventArgs(string data)
        {
            this.Data = data;
        }

        public string Data { get; private set; }
    }
}
