using ProcessMonitor.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessMonitor
{
    class Program
    {
        const int BYTES_IN_MB = 1000000;

        private static JobObject _jobObject;
        private static IProcess _process;
        private static bool _processExited = false;

        static void Main(string[] args)
        {
            var envVariables= EnvironmentBlock.Create(Environment.GetEnvironmentVariables()).ToDictionary();
            envVariables.Add("PORT", "8081");
  
            IProcessRunner processRunner = new ProcessRunner();
            ProcessRunSpec runSpec = new ProcessRunSpec()
            {
                Environment = envVariables,
                ExecutablePath = @"C:\containerizer\hwc.exe",
                Arguments = new[] { "-appRootPath", "env\\WebApiMemoryLimit" },
                WorkingDirectory = @"C:\containerizer\",
              
                OutputCallback = (data) =>
                {
                    Console.WriteLine($"HWC OUT {data}");
                },
                ErrorCallback = (data) =>
                {
                    Console.WriteLine($"HWC ERR {data}");
                },
            };
            runSpec.ExitHandler += Process_Exited;
            var _process = processRunner.Run(runSpec);

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
            limits.LimitMemory(256 * BYTES_IN_MB);
            limits.MemoryLimitReached += Limits_MemoryLimitReached;
            Console.WriteLine($"Memory Limit Set:{_jobObject.GetJobMemoryLimit()/ BYTES_IN_MB} MB");

            // monitor memory peak 
            while(!_processExited)
            {
                Console.WriteLine($"Job Peak Memory: {_jobObject.GetPeakJobMemoryUsed()/ BYTES_IN_MB} MB");
                Thread.Sleep(5 * 1000);
            }

            Console.WriteLine("Process complete.");
            Console.ReadKey();
        }

        private static void Process_Exited(object sender, EventArgs e)
        {
            var process = (Process)sender;
            Console.WriteLine($"hwc exit code {process.ExitCode}");
            _processExited = true;
        }

        private static void Limits_MemoryLimitReached(object sender, EventArgs e)
        {
            Console.Error.WriteLine("Memory Limit Reached! Terminating Job...");
           // _jobObject.TerminateProcessesAndWait();
            Console.Error.WriteLine("Process terminated");
        }
    }
}
