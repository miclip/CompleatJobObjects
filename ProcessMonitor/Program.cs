using ProcessMonitor.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessMonitor
{
    class Program
    {
        const int BYTES_IN_MEG = 1000000;

        private static JobObject _jobObject;
        private static Process _process;

        static void Main(string[] args)
        {
            // start hwc
            var startInfo = new ProcessStartInfo(@"C:\containerizer\hwc.exe", @"-appRootPath env\WebApiMemoryLimit");
            startInfo.WorkingDirectory = @"C:\containerizer\";            
            Environment.SetEnvironmentVariable("PORT", "8081");
            _process = Process.Start(startInfo);
            _process.EnableRaisingEvents = true;

            // try to capture process exit code on failure
            _process.Exited += Process_Exited;

            // assign process to JO
            _jobObject = new JobObject(null);            
            _jobObject.AssignProcessToJob(_process.Handle);

   
            Task.Run(() =>
            {
                var currentStatus = JobObject.CompletionMsg.NoCompletionStatus;
                while (currentStatus!= JobObject.CompletionMsg.ActiveProcessZero)
                {
                    foreach (var msg in _jobObject.GetQueuedCompletionStatus())
                    {
                        Console.WriteLine($"Completion Message:{msg.ToString()}");
                        currentStatus = msg;
                    }

                }

                Console.WriteLine("Process no longer exists!");
            });
            
            // set limits 
            JobObjectLimits limits = new JobObjectLimits(_jobObject, new TimeSpan(0, 0, 10));
            limits.LimitMemory(256 * BYTES_IN_MEG);
            limits.MemoryLimitReached += Limits_MemoryLimitReached;
            Console.WriteLine($"Memory Limit Set:{_jobObject.GetJobMemoryLimit()/ BYTES_IN_MEG} MB");

            // monitor memory peak 
            while(true)
            {
                Console.WriteLine($"Job Peak Memory: {_jobObject.GetPeakJobMemoryUsed()/ BYTES_IN_MEG} MB");
                Thread.Sleep(5 * 1000);
            }
        }

        private static void Process_Exited(object sender, EventArgs e)
        {
            Console.WriteLine($"hwc exit code {_process.ExitCode}");
        }

        private static void Limits_MemoryLimitReached(object sender, EventArgs e)
        {
            Console.Error.WriteLine("Memory Limit Reached! Terminating Job...");
           // _jobObject.TerminateProcessesAndWait();
            Console.Error.WriteLine("Process terminated");
        }
    }
}
