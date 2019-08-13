using System;
using System.Runtime.InteropServices;

public class Program
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);
    
    static void Main()
    {
        MessageBox(new IntPtr(0), "Hello World from user32's MessageBox!!", "Important Dialog", 0);
    }
}