//Taken from https://www.codeproject.com/Articles/769741/Csharp-AES-bits-Encryption-Library-with-Salt

using System.Security.Cryptography;
using System.IO;
using System.Text;
using System;

public class Program
{

    static void Main()
    {
        byte[] shellcode = new byte[1] { 0xfc };

        byte[] passwordBytes = Encoding.UTF8.GetBytes("pass");

        passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

        byte[] bytesEncrypted = AES_Encrypt(shellcode, passwordBytes);

        StringBuilder newshellcode = new StringBuilder();
        newshellcode.Append("byte[] shellcode = new byte[");
        newshellcode.Append(bytesEncrypted.Length);
        newshellcode.Append("] { ");
        for (int i = 0; i < bytesEncrypted.Length; i++)
        {
            newshellcode.Append("0x");
            newshellcode.AppendFormat("{0:x2}", bytesEncrypted[i]);
            if (i < bytesEncrypted.Length - 1)
            {
                newshellcode.Append(", ");
            }

        }
        newshellcode.Append(" };");
        Console.WriteLine(newshellcode.ToString());
        Console.WriteLine("");
        Console.WriteLine("");

        byte[] decrypted = AES_Decrypt(bytesEncrypted, passwordBytes);

        StringBuilder newshellcode2 = new StringBuilder();
        newshellcode2.Append("byte[] shellcode2 = new byte[");
        newshellcode2.Append(decrypted.Length);
        newshellcode2.Append("] { ");
        for (int i = 0; i < decrypted.Length; i++)
        {
            newshellcode2.Append("0x");
            newshellcode2.AppendFormat("{0:x2}", decrypted[i]);
            if (i < decrypted.Length - 1)
            {
                newshellcode2.Append(", ");
            }

        }
        newshellcode2.Append(" };");
        Console.WriteLine(newshellcode2.ToString());




    }

    public static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
    {
        byte[] encryptedBytes = null;

        byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        using (MemoryStream ms = new MemoryStream())
        {
            using (RijndaelManaged AES = new RijndaelManaged())
            {
                AES.KeySize = 256;
                AES.BlockSize = 128;

                var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);

                AES.Mode = CipherMode.CBC;

                using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                    cs.Close();
                }
                encryptedBytes = ms.ToArray();
            }
        }

        return encryptedBytes;
    }

    public static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
    {
        byte[] decryptedBytes = null;
        byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        using (MemoryStream ms = new MemoryStream())
        {
            using (RijndaelManaged AES = new RijndaelManaged())
            {
                AES.KeySize = 256;
                AES.BlockSize = 128;

                var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);

                AES.Mode = CipherMode.CBC;

                using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                    cs.Close();
                }
                decryptedBytes = ms.ToArray();
            }
        }

        return decryptedBytes;
    }
}