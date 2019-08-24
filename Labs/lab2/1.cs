using System;
using System.Text;
using System.Net;

class Program
{
    static void Main()
    {
        WebClient wclient = new WebClient();
        wclient.Headers["User-Agent"] ="Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36";
        byte[] response = wclient.DownloadData("https://www.google.com/");
        Console.WriteLine("Downloaded Bytes");
        Console.WriteLine(response.Length);
        string html = Encoding.ASCII.GetString(response);
        Console.WriteLine("HTML Content");
        Console.WriteLine(html);
    }
}