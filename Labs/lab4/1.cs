
using System;
using System.IO;
using System.Text;


public class Program
{

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
        string key = "ABCDE";

        byte[] shellcode = new byte[1] { 0xfc };
        byte[] xorshellcode;

        xorshellcode = xor(shellcode, Encoding.ASCII.GetBytes(key));
        StringBuilder newshellcode = new StringBuilder();
        newshellcode.Append("byte[] shellcode = new byte[");
        newshellcode.Append(xorshellcode.Length);
        newshellcode.Append("] { ");
        for (int i = 0; i < xorshellcode.Length; i++)
        {
            newshellcode.Append("0x");
            newshellcode.AppendFormat("{0:x2}", xorshellcode[i]);
            if (i < xorshellcode.Length - 1)
            {
                newshellcode.Append(", ");
            }

        }
        newshellcode.Append(" };");
        Console.WriteLine(newshellcode.ToString());


        return;
    }
}