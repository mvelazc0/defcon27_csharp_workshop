using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;


public class Program
{


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
        Process pickedproc = Process.GetProcessById(val);
        ProcessModule myProcessModule;
        ProcessModuleCollection myProcessModuleCollection = pickedproc.Modules;
        Console.WriteLine("Loaded Modules by " + pickedproc.MainModule.FileName);
        Console.WriteLine("--------------------------------------------------------------------\n");
        for (int i = 0; i < myProcessModuleCollection.Count; i++)
        {
            myProcessModule = myProcessModuleCollection[i];
            Console.WriteLine(myProcessModule.FileName);
        }


    }
}