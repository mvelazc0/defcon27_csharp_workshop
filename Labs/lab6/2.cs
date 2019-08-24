using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;


public class Program
{

    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

    const int PROCESS_CREATE_THREAD = 0x0002;
    const int PROCESS_QUERY_INFORMATION = 0x0400;
    const int PROCESS_VM_OPERATION = 0x0008;
    const int PROCESS_VM_WRITE = 0x0020;
    const int PROCESS_VM_READ = 0x0010;

    const uint MEM_COMMIT = 0x00001000;
    const uint MEM_RESERVE = 0x00002000;
    const uint PAGE_READWRITE = 4;


    public static void Main()
    {
        Console.WriteLine("Listing all processes...");
        Console.WriteLine("--------------------------------------------------------------------");

        Process[] procs = Process.GetProcesses();
        foreach (Process proc in procs)
        {
            try
            {
                Console.WriteLine("Name:" + proc.ProcessName + " Path:" + proc.MainModule.FileName + " Id:" + proc.Id);
            }
            catch
            {
                continue;
            }
        }
        Console.WriteLine("--------------------------------------------------------------------\n");
        Console.WriteLine("Enter Id to inspect:");
        int val;
        val = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine(val);
        Process proc1 = Process.GetProcessById(val);

        Console.WriteLine("Getting handle to process " + proc1.MainModule.FileName);
        IntPtr procHandle = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, proc1.Id);
        Console.WriteLine("Got procHandle: " + procHandle);

        string blob = "ABCD1234AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

        Console.WriteLine("Allocating memory in " + proc1.MainModule.FileName);
        IntPtr memAddr = VirtualAllocEx(procHandle, IntPtr.Zero, (uint)((blob.Length + 1) * Marshal.SizeOf(typeof(char))), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
        Console.WriteLine("Done.");

        Console.WriteLine("Writing to process memory");
        UIntPtr bytesWritten;
        bool resp1 = WriteProcessMemory(procHandle, memAddr, Encoding.Default.GetBytes(blob), (uint)((blob.Length + 1) * Marshal.SizeOf(typeof(char))), out bytesWritten);
        Console.WriteLine("Done.");





    }
}