//https://github.com/leoloobeek/csharp/blob/master/ExecutionTesting.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Diagnostics;

class Program
{
    static void Main()
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
        Console.WriteLine("Enter Id:");
        int ParentProcId;
        ParentProcId = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine(ParentProcId);

        string binaryPath = "C:\\Windows\\System32\\notepad.exe";

        Console.WriteLine(String.Format("Press enter to execute '{0}' under pid {1}", binaryPath, ParentProcId));
        Console.ReadKey();
        SpoofParent.Run(ParentProcId, binaryPath);
        Console.WriteLine("Done. Press any key to exit...");
        Console.ReadKey();
    }
}

class SpoofParent
{
    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes, ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFOEX lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern UInt32 WaitForSingleObject(IntPtr handle, UInt32 milliseconds);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UpdateProcThreadAttribute(IntPtr lpAttributeList, uint dwFlags, IntPtr Attribute, IntPtr lpValue, IntPtr cbSize, IntPtr lpPreviousValue, IntPtr lpReturnSize);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool InitializeProcThreadAttributeList(IntPtr lpAttributeList, int dwAttributeCount, int dwFlags, ref IntPtr lpSize);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool SetHandleInformation(IntPtr hObject, HANDLE_FLAGS dwMask, HANDLE_FLAGS dwFlags);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, IntPtr hSourceHandle, IntPtr hTargetProcessHandle, ref IntPtr lpTargetHandle, uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwOptions);

    public static bool Run(int parentProcessId, string binaryPath)
    {
        // STARTUPINFOEX members
        const int PROC_THREAD_ATTRIBUTE_PARENT_PROCESS = 0x00020000;

        // STARTUPINFO members (dwFlags and wShowWindow)
        const int STARTF_USESTDHANDLES = 0x00000100;
        const int STARTF_USESHOWWINDOW = 0x00000001;
        const short SW_HIDE = 0x0000;

        // dwCreationFlags
        const uint EXTENDED_STARTUPINFO_PRESENT = 0x00080000;
        const uint CREATE_NO_WINDOW = 0x08000000;

        var pInfo = new PROCESS_INFORMATION();
        var siEx = new STARTUPINFOEX();

        //siEx.StartupInfo.cb = Marshal.SizeOf(siEx);
        IntPtr lpValueProc = IntPtr.Zero;
        IntPtr hSourceProcessHandle = IntPtr.Zero;
        var lpSize = IntPtr.Zero;

        InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref lpSize);
        siEx.lpAttributeList = Marshal.AllocHGlobal(lpSize);
        InitializeProcThreadAttributeList(siEx.lpAttributeList, 1, 0, ref lpSize);

        IntPtr parentHandle = OpenProcess(ProcessAccessFlags.CreateProcess | ProcessAccessFlags.DuplicateHandle, false, parentProcessId);

        lpValueProc = Marshal.AllocHGlobal(IntPtr.Size);
        Marshal.WriteIntPtr(lpValueProc, parentHandle);

        UpdateProcThreadAttribute(siEx.lpAttributeList, 0, (IntPtr)PROC_THREAD_ATTRIBUTE_PARENT_PROCESS, lpValueProc, (IntPtr)IntPtr.Size, IntPtr.Zero, IntPtr.Zero);

        siEx.StartupInfo.dwFlags = STARTF_USESHOWWINDOW | STARTF_USESTDHANDLES;
        siEx.StartupInfo.wShowWindow = SW_HIDE;

        var ps = new SECURITY_ATTRIBUTES();
        var ts = new SECURITY_ATTRIBUTES();
        ps.nLength = Marshal.SizeOf(ps);
        ts.nLength = Marshal.SizeOf(ts);
        bool ret = CreateProcess(binaryPath, null, ref ps, ref ts, true, EXTENDED_STARTUPINFO_PRESENT | CREATE_NO_WINDOW, IntPtr.Zero, null, ref siEx, out pInfo);
        if (!ret)
        {
            Console.WriteLine("[!] Proccess failed to execute!");
            return false;
        }

        return true;


    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct STARTUPINFOEX
    {
        public STARTUPINFO StartupInfo;
        public IntPtr lpAttributeList;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct STARTUPINFO
    {
        public Int32 cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public Int32 dwX;
        public Int32 dwY;
        public Int32 dwXSize;
        public Int32 dwYSize;
        public Int32 dwXCountChars;
        public Int32 dwYCountChars;
        public Int32 dwFillAttribute;
        public Int32 dwFlags;
        public Int16 wShowWindow;
        public Int16 cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public int dwProcessId;
        public int dwThreadId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SECURITY_ATTRIBUTES
    {
        public int nLength;
        public IntPtr lpSecurityDescriptor;
        [MarshalAs(UnmanagedType.Bool)]
        public bool bInheritHandle;
    }

    [Flags]
    public enum ProcessAccessFlags : uint
    {
        All = 0x001F0FFF,
        Terminate = 0x00000001,
        CreateThread = 0x00000002,
        VirtualMemoryOperation = 0x00000008,
        VirtualMemoryRead = 0x00000010,
        VirtualMemoryWrite = 0x00000020,
        DuplicateHandle = 0x00000040,
        CreateProcess = 0x000000080,
        SetQuota = 0x00000100,
        SetInformation = 0x00000200,
        QueryInformation = 0x00000400,
        QueryLimitedInformation = 0x00001000,
        Synchronize = 0x00100000
    }

    [Flags]
    enum HANDLE_FLAGS : uint
    {
        None = 0,
        INHERIT = 1,
        PROTECT_FROM_CLOSE = 2
    }
}