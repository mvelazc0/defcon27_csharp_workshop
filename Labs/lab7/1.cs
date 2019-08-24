using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
using System.IO;
using System.Text;
using System.Threading;


public class Program
{
    //https://docs.microsoft.com/en-us/windows/desktop/api/processthreadsapi/nf-processthreadsapi-openthread
    [DllImport("kernel32.dll")]
    static extern IntPtr OpenThread(uint dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

    //https://docs.microsoft.com/en-us/windows/desktop/api/processthreadsapi/nf-processthreadsapi-suspendthread
    [DllImport("kernel32.dll")]
    static extern uint SuspendThread(IntPtr hThread);

    //https://docs.microsoft.com/en-us/windows/desktop/api/processthreadsapi/nf-processthreadsapi-resumethread
    [DllImport("kernel32.dll")]
    static extern int ResumeThread(IntPtr hThread);


    private static UInt32 SUSPEND_RESUME = 0x0002;

    public static void Main()
    {
        string proc = "msiexec.exe";
        Process newproc;
        newproc = Process.Start(proc);

        Console.WriteLine("Started " + proc + " with Process Id:" + newproc.Id);
        Console.WriteLine("Press Key to suspend the process ...");
        Console.ReadKey();
        Console.WriteLine("Suspending process...");
        foreach (ProcessThread thread in newproc.Threads)
        {
            IntPtr pOpenThread;
            pOpenThread = OpenThread(SUSPEND_RESUME, false, (uint)thread.Id);
            if (pOpenThread == IntPtr.Zero)
            {
                break;
            }
            SuspendThread(pOpenThread);
        }
        Console.WriteLine("Suspended!");
        Console.WriteLine("Press Key to resume the process ...");
        Console.ReadKey();
        Console.WriteLine("Resuming process...");
        foreach (ProcessThread thread in newproc.Threads)
        {
            IntPtr pOpenThread;
            pOpenThread = OpenThread(SUSPEND_RESUME, false, (uint)thread.Id);
            if (pOpenThread == IntPtr.Zero)
            {
                break;
            }
            ResumeThread(pOpenThread);
        }
        Console.WriteLine("Resumed!");

    }
}