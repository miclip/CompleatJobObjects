using ProcessMonitor.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMonitor
{
    class Program
    {
        const int BYTESINMEG = 1000000;
        private static JobObject jobObject;
        private static Process process;
        static void Main(string[] args)
        {
            // start hwc
            var startInfo = new ProcessStartInfo(@"C:\containerizer\hwc.exe", @"-appRootPath env\WebApiMemoryLimit");
            startInfo.WorkingDirectory = @"C:\containerizer\";            
            Environment.SetEnvironmentVariable("PORT", "8081");
            process = Process.Start(startInfo);

            // try to capture process exit code on failure
            process.Exited += Process_Exited;

            // assign process to JO
            jobObject = new JobObject(null);            
            jobObject.AssignProcessToJob(process.Handle);
            
            // set limits 
            JobObjectLimits limits = new JobObjectLimits(jobObject, new TimeSpan(0, 0, 10));
            limits.LimitMemory(256 * BYTESINMEG);
            limits.MemoryLimitReached += Limits_MemoryLimitReached;
            Console.WriteLine($"Memory Limit Set:{jobObject.GetJobMemoryLimit()/ BYTESINMEG}");

            // monitor memory peak 
            while(true)
            {
                Console.WriteLine($"Job Peak Memory: {jobObject.GetPeakJobMemoryUsed()/ BYTESINMEG}");
                System.Threading.Thread.Sleep(5 * 1000);
            }
        }

        private static void Process_Exited(object sender, EventArgs e)
        {
            Console.WriteLine($"hwc exit code {process.ExitCode}");
        }

        private static void Limits_MemoryLimitReached(object sender, EventArgs e)
        {
            Console.Error.WriteLine("Memory Limit Reached!");
            jobObject.TerminateProcessesAndWait(1000);
            Console.Error.WriteLine("Process terminated");
        }
    }
}
