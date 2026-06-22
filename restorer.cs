using System;
using System.IO;
using System.Text;

class FileRestorer
{
    static void Main()
    {
        string path = args[0];
        string content = File.ReadAllText(args[1], Encoding.UTF8);
        File.WriteAllText(path, content, new UTF8Encoding(false));
        Console.WriteLine("Restored " + path);
    }
}