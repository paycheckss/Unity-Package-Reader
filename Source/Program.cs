using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class UnityPackageReader
{
    static void Main(string[] args)
    {
        Console.Write("Enter path to .unitypackage file: ");
        string packagePath = Console.ReadLine();

        if (!File.Exists(packagePath) || !packagePath.EndsWith(".unitypackage"))
        {
            Console.WriteLine("Invalid file path.");
            return;
        }

        Console.WriteLine("Reading and decompressing...");

        Dictionary<string, byte[]> files;
        using (FileStream fs = new FileStream(packagePath, FileMode.Open))
        using (GZipStream gzip = new GZipStream(fs, CompressionMode.Decompress))
        using (MemoryStream tarStream = new MemoryStream())
        {
            gzip.CopyTo(tarStream);
            tarStream.Position = 0;
            files = ReadTarEntries(tarStream);
        }

        List<(string path, byte[] content)> scripts = new List<(string, byte[])>();

        foreach (var kvp in files.Where(f => f.Key.EndsWith("pathname")))
        {
            string assetPath = Encoding.UTF8.GetString(kvp.Value).Trim();
            string baseFolder = kvp.Key.Substring(0, kvp.Key.LastIndexOf("/"));
            string assetFile = baseFolder + "/asset";

            if (assetPath.EndsWith(".cs") && files.ContainsKey(assetFile))
            {
                scripts.Add((assetPath, files[assetFile]));
            }
        }

        Console.WriteLine($"\nFound {scripts.Count} script(s):");
        for (int i = 0; i < scripts.Count; i++)
        {
            Console.WriteLine($"[{i}] {scripts[i].path}");
        }

        Console.Write("\nExtract all scripts? (y/n): ");
        if (Console.ReadLine().ToLower() == "y")
        {
            foreach (var script in scripts)
                SaveScript(script.path, script.content);
            Console.WriteLine("\nScripts extracted to ./ExtractedScripts/");
        }
        else
        {
            Console.Write("Enter index of script to extract: ");
            if (int.TryParse(Console.ReadLine(), out int index) && index >= 0 && index < scripts.Count)
            {
                SaveScript(scripts[index].path, scripts[index].content);
                Console.WriteLine($"Extracted: {scripts[index].path}");
            }
            else
            {
                Console.WriteLine("Invalid index.");
            }
        }
    }

    static void SaveScript(string path, byte[] content)
    {
        string outPath = Path.Combine("ExtractedScripts", path);
        Directory.CreateDirectory(Path.GetDirectoryName(outPath));
        File.WriteAllBytes(outPath, content);
    }

    static Dictionary<string, byte[]> ReadTarEntries(Stream stream)
    {
        var entries = new Dictionary<string, byte[]>();
        byte[] header = new byte[512];

        while (true)
        {
            int bytesRead = stream.Read(header, 0, 512);
            if (bytesRead < 512 || header.All(b => b == 0)) break;

            string name = Encoding.ASCII.GetString(header, 0, 100).Trim('\0');
            string sizeStr = Encoding.ASCII.GetString(header, 124, 12).Trim('\0').Trim();
            long size = Convert.ToInt64(sizeStr, 8);

            byte[] content = new byte[size];
            stream.Read(content, 0, content.Length);
            entries[name] = content;

            long padding = (512 - (size % 512)) % 512;
            stream.Position += padding;
        }

        return entries;
    }
}
